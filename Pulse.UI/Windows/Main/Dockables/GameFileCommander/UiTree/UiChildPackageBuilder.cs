using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiChildPackageBuilder
    {
        private enum DbFileKind
        {
            Unknown,
            ZtrContainer,
            WpdTable,
        }

        private readonly ConcurrentDictionary<UiArchiveExtension, ConcurrentBag<UiNode>> _nodes = new();
        private readonly ConcurrentDictionary<string, Pair<ArchiveEntry, ArchiveEntry>> _pairs = new();

        private readonly List<ZtrTextUnitFile> _textFiles = new();
        private readonly Dictionary<string, ArchiveEntry> _textEntries = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _textUnitEntries = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<ZtrTextUnit> _textUnits = new();
        private readonly Dictionary<string, DbFileKind> _dbFileKinds = new(StringComparer.OrdinalIgnoreCase);

        private readonly string _areasDirectory;
        private readonly bool _allowZones;

        public UiChildPackageBuilder(string areasDirectory, bool allowZones = true)
        {
            _areasDirectory = areasDirectory;
            _allowZones = allowZones;
        }

        public void CollectTextUnits(ArchiveListing listing)
        {
            if (InteractionService.GamePart != FFXIIIGamePart.Part3)
                return;

            foreach (ArchiveEntry entry in listing)
                CollectTextUnit(listing, entry, entry.Name.ToLowerInvariant());

            foreach (ZtrTextUnit unit in ZtrTextUnitScanner.Scan(_textFiles))
            {
                if (!unit.Files.ContainsKey("us"))
                    continue;

                _textUnits.Add(unit);

                foreach (string path in unit.Files.Values)
                    _textUnitEntries.Add(path);
            }
        }

        public bool IsTextUnit(string entryPath)
        {
            return _textUnitEntries.Contains(entryPath);
        }

        public bool TryAdd(ArchiveListing listing, ArchiveEntry entry, string entryPath, string entryName, out UiNode childNode)
        {
            childNode = null;

            if (TryAddZoneListing(listing, entry, entryPath, out childNode))
                return true;

            if (TryAddDbFiles(listing, entry, entryPath, entryName, out childNode))
                return true;

            if (TryAddMoviesListing(listing, entry, entryName))
                return true;

            if (TryAddImgbPair(listing, entry, entryPath, entryName))
                return true;

            return false;
        }

        public UiContainerNode TryBuild(ArchiveListing listing)
        {
            if (_nodes.Count == 0 && _textUnits.Count == 0)
                return null;

            BuildTextUnits(listing);

            int counter = 0;
            UiContainerNode result = new(Lang.Dockable.GameFileCommander.ArchivesNode, UiNodeType.Group);
            UiNode[] extensions = new UiNode[_nodes.Count];
            foreach (KeyValuePair<UiArchiveExtension, ConcurrentBag<UiNode>> pair in _nodes)
            {
                UiContainerNode extensionNode = BuildExtensionNode(pair.Key, pair.Value);
                extensionNode.Parent = result;
                extensions[counter++] = extensionNode;
            }
            result.SetChilds(extensions);
            return result;
        }

        private UiContainerNode BuildExtensionNode(UiArchiveExtension key, ConcurrentBag<UiNode> entries)
        {
            string separator = Path.AltDirectorySeparatorChar.ToString();

            UiContainerNode extensionNode = new(key.ToString().ToUpper(), UiNodeType.Group);
            Dictionary<string, UiContainerNode> dirs = new(entries.Count);
            foreach (UiNode leaf in entries)
            {
                UiNode parent = extensionNode;
                string[] path = leaf.Name.ToLowerInvariant().Split(Path.AltDirectorySeparatorChar);
                for (int i = 0; i < path.Length - 1; i++)
                {
                    UiContainerNode directory;
                    string directoryName = path[i];
                    string directoryPath = string.Join(separator, path, 0, i + 1);
                    if (!dirs.TryGetValue(directoryPath, out directory))
                    {
                        directory = new(directoryName, UiNodeType.Directory) {Parent = parent};
                        dirs.Add(directoryPath, directory);
                    }
                    parent = directory;
                }
                leaf.Parent = parent;
                leaf.Name = path[path.Length - 1];
            }

            //foreach (IGrouping<UiNode, UiNode> leafs in entries.GroupBy(e => e.Parent))
            //    ((UiContainerNode)leafs.Key).SetChilds(leafs.ToArray());

            UiNode[] childs = null;
            foreach (IGrouping<UiNode, UiNode> leafs in dirs.Values.Union(entries).GroupBy(e => e.Parent))
            {
                if (leafs.Key == extensionNode)
                {
                    childs = leafs.ToArray();
                    continue;
                }

                ((UiContainerNode)leafs.Key).SetChilds(leafs.ToArray());
            }

            foreach (UiContainerNode node in dirs.Values)
                node.AbsorbSingleChildContainer();

            extensionNode.SetChilds(childs);
            return extensionNode;
        }

        private ConcurrentBag<UiNode> ProvideRootNodeChilds(UiArchiveExtension extension)
        {
            return _nodes.GetOrAdd(extension, e => new());
        }

        private Pair<ArchiveEntry, ArchiveEntry> ProvidePair(string entryPathWithoutExtension)
        {
            return _pairs.GetOrAdd(entryPathWithoutExtension, p => new());
        }

        private bool TryAddDbFiles(ArchiveListing listing, ArchiveEntry entry, string entryPath, string entryName, out UiNode childNode)
        {
            childNode = null;

            if (!entryPath.Contains("db/ai/npc/pack")) return false;

            if (InteractionService.GamePart == FFXIIIGamePart.Part3)
            {
                if (DetectDbFileKind(listing, entry, entryPath) != DbFileKind.ZtrContainer)
                    return false;

                childNode = new UiFileTableNode(listing, UiArchiveExtension.Bin, entry, entry, false, entryName);
                return true;
            }

            UiArchiveExtension extension = UiArchiveExtension.Bin;

            UiFileTableNode node = new(listing, extension, entry, entry, false);
            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(extension);
            container.Add(node);
            return true;
        }

        private DbFileKind DetectDbFileKind(ArchiveListing listing, ArchiveEntry entry, string entryPath)
        {
            if (_dbFileKinds.TryGetValue(entryPath, out DbFileKind cached))
                return cached;

            DbFileKind kind = DbFileKind.Unknown;

            try
            {
                using (Stream input = listing.Accessor.ExtractBinary(entry))
                {
                    if (ZtrContainer.TryDetect(input, entryPath, out ZtrContainerType _))
                    {
                        kind = DbFileKind.ZtrContainer;
                    }
                    else if (input.CanSeek && TryReadWpdHeader(input, out WpdHeader _))
                    {
                        kind = DbFileKind.WpdTable;
                    }
                }
            }
            catch (Exception)
            {
                kind = DbFileKind.Unknown;
            }

            _dbFileKinds[entryPath] = kind;
            return kind;
        }

        private static bool TryReadWpdHeader(Stream input, out WpdHeader header)
        {
            header = null;

            try
            {
                input.Position = 0;
                header = ZtrContainer.ReadHeader(input);
            }
            catch (Exception)
            {
                header = null;
            }

            return header != null && header.Entries != null && header.Entries.Length > 0;
        }

        private void CollectTextUnit(ArchiveListing listing, ArchiveEntry entry, string entryPath)
        {
            if (!ZtrTextUnitScanner.TryGetContainerType(entryPath, out ZtrContainerType type))
                return;

            if (type != ZtrContainerType.Loose)
                return;

            if (!entryPath.EndsWith(".ztr", StringComparison.OrdinalIgnoreCase))
                return;

            _textFiles.Add(ZtrTextUnitFile.ProvideLoose(entryPath));
            _textEntries[entryPath] = entry;
        }

        private void BuildTextUnits(ArchiveListing listing)
        {
            if (_textUnits.Count == 0)
                return;

            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(UiArchiveExtension.Ztr);
            foreach (ZtrTextUnit unit in _textUnits)
            {
                Dictionary<string, ArchiveEntry> entries = new(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, string> pair in unit.Files)
                {
                    if (_textEntries.TryGetValue(pair.Value, out ArchiveEntry entry))
                        entries.Add(pair.Key, entry);
                }

                container.Add(new UiTextUnitNode(listing, unit, entries));
            }
        }
        private bool TryAddZoneListing(ArchiveListing parentListing, ArchiveEntry entry, string entryPath, out UiNode childNode)
        {
            childNode = null;

            if (!_allowZones)
                return false;

            if (_areasDirectory == null)
                return false;

            if (!entryPath.Contains("zone/filelist"))
                return false;

            // Slip an empty archives
            if (entryPath.EndsWith("2"))
                return false;

            string key = ExtractZoneKey(entryPath);
            if (key == null)
                return false;

            string binaryName = $"white_{key}_img.win32.bin";

            string binaryPath = Path.Combine(_areasDirectory, binaryName);
            if (!File.Exists(binaryPath))
                return false;

            ArchiveAccessor accessor = parentListing.Accessor.CreateDescriptor(binaryPath, entry);
            childNode = new UiArchiveNode(accessor, parentListing, false);

            return true;
        }

        private static string ExtractZoneKey(string entryPath)
        {
            const string prefix = "filelist_";

            int index = entryPath.LastIndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            int start = index + prefix.Length;
            int end = entryPath.IndexOf('.', start);
            if (end < 0)
                end = entryPath.Length;

            if (end <= start)
                return null;

            return entryPath.Substring(start, end - start);
        }

        private bool TryAddMoviesListing(ArchiveListing parentListing, ArchiveEntry entry, string entryName)
        {
            switch (entryName)
            {
                case "movie_items.win32.wdb":
                case "movie_items_us.win32.wdb":
                    break;
                default:
                    return false;
            }

            UiArchiveExtension extension = GetArchiveExtension(entry);

            UiDataTableNode node = new(parentListing, extension, entry);
            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(extension);
            container.Add(node);

            return true;
        }

        private bool TryAddImgbPair(ArchiveListing listing, ArchiveEntry entry, string entryPath, string entryName)
        {
            string ext = PathEx.GetMultiDotComparableExtension(entryName);
            switch (ext)
            {
                case ".win32.xfv":
                case ".win32.xgr":
                case ".win32.xwb":
                    break;
                case ".win32.trb":
                    break;
                case ".win32.imgb":
                    break;
                default:
                    return false;
            }

            string longName = entryPath.Substring(0, entryPath.Length - ext.Length);
            if (IsUnexpectedEntry(listing.Name, longName))
                return false;

            return SetPairedEntry(listing, entry, ext, longName);
        }

        private bool SetPairedEntry(ArchiveListing listing, ArchiveEntry entry, string ext, string longName)
        {
            Pair<ArchiveEntry, ArchiveEntry> pair = ProvidePair(longName);

            switch (ext)
            {
                case ".win32.imgb":
                    pair.Item2 = entry;
                    break;
                default:
                    pair.Item1 = entry;
                    break;
            }

            if (pair.IsAnyEmpty) return true;
            UiArchiveExtension extension = GetArchiveExtension(pair.Item1);

            UiFileTableNode node = new(listing, extension, pair.Item1, pair.Item2);
            ConcurrentBag<UiNode> container = ProvideRootNodeChilds(extension);
            container.Add(node);

            return true;
        }

        private static UiArchiveExtension GetArchiveExtension(ArchiveEntry indices)
        {
            string ext = PathEx.GetMultiDotComparableExtension(indices.Name);

            const string extensionPrefix = ".win32.";
            ext = ext.Substring(extensionPrefix.Length);
            return EnumCache<UiArchiveExtension>.Parse(ext);
        }

        private static bool IsUnexpectedEntry(string listingName, string longName)
        {
            if (InteractionService.GamePart != FFXIIIGamePart.Part1)
                return false;

            const string zoneFileListPrefix = @"zone/filelist_z";
            const string zoneBgLogPrefix = @"bg/loc";

            if (listingName.StartsWith(zoneFileListPrefix) && longName.StartsWith(zoneBgLogPrefix))
            {
                if (listingName.Substring(zoneFileListPrefix.Length, 3) != longName.Substring(zoneBgLogPrefix.Length, 3))
                    return true;
            }

            return false;
        }
    }
}