using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveNode : UiLazyContainerNode
    {
        private readonly ArchiveAccessor _accessor;
        private readonly ArchiveListing _parentListing;
        private readonly bool _allowZones;
        private ArchiveListing _listing;

        public UiArchiveNode(ArchiveAccessor accessor, ArchiveListing parentListing, bool allowZones = true)
            : base(accessor.ListingEntry.Name, UiNodeType.Archive)
        {
            _accessor = accessor;
            _parentListing = parentListing;
            _allowZones = allowZones;
        }

        protected override ImageSource GetIcon()
        {
            return Icons.DiskIcon;
        }

        protected override UiNode[] ExpandChilds()
        {
            switch (InteractionService.GamePart)
            {
                case FFXIIIGamePart.Part1:
                    _listing = ArchiveListingReaderV1.Read(_accessor, null, null);
                    break;
                case FFXIIIGamePart.Part2:
                    _listing = ArchiveListingReaderV2.Read(_accessor, null, null);
                    break;
                case FFXIIIGamePart.Part3:
                    _listing = ArchiveListingReaderV3.Read(_accessor, null, null);
                    break;
                default:
                    throw new NotSupportedException(InteractionService.GamePart.ToString());
            }

            _listing.Parent = _parentListing;
            
            HashSet<string> set = new();
            Dictionary<string, UiNode> dic = new(_listing.Count * 2);

            UiChildPackageBuilder childPackages = new(InteractionService.GameLocation.Provide().AreasDirectory, _allowZones);
            childPackages.CollectTextUnits(_listing);

            foreach (ArchiveEntry entry in _listing)
            {
                UiNode parent = this;

                string entryPath = entry.Name.ToLowerInvariant();
                string treePath = NormalizeEntryPath(entryPath);
                if (treePath == null)
                    continue;

                int index = treePath.LastIndexOf(Path.AltDirectorySeparatorChar);
                string name;
                if (index > 0)
                {
                    name = treePath.Substring(index + 1);
                }
                else
                {
                    name = treePath;
                }

                childPackages.TryAdd(_listing, entry, entryPath, name, out UiNode childNode);

                bool isTextUnit = childPackages.IsTextUnit(entryPath);

                if (index > 0)
                {
                    string directoryPath = treePath.Substring(0, index);
                    set.Add(directoryPath);
                    if (!dic.TryGetValue(directoryPath, out parent))
                    {
                        string directoryName = directoryPath;
                        int nameIndex = directoryPath.LastIndexOf(Path.AltDirectorySeparatorChar);
                        if (nameIndex > 0)
                            directoryName = directoryPath.Substring(nameIndex + 1);

                        parent = new UiContainerNode(directoryName, UiNodeType.Directory);
                        dic.Add(directoryPath, parent);
                    }
                }

                if (dic.ContainsKey(treePath))
                    continue;

                if (childNode != null)
                {
                    if (childNode is UiArchiveNode zoneNode)
                        zoneNode.Name = PathEx.ChangeMultiDotExtension(name, null);

                    childNode.Parent = parent;
                    dic.Add(treePath, childNode);
                    continue;
                }

                dic.Add(treePath, new UiArchiveLeaf(name, entry, _listing) {Parent = parent, IsTextUnit = isTextUnit});
            }

            UiContainerNode packagesNode = childPackages.TryBuild(_listing);
            if (packagesNode != null)
            {
                packagesNode.Parent = this;
                dic.Add(packagesNode.Name, packagesNode);
            }

            string separator = Path.AltDirectorySeparatorChar.ToString();
            foreach (string dir in set)
            {
                UiNode parent = this;

                string[] parts = dir.Split(Path.AltDirectorySeparatorChar);
                for (int i = 0; i < parts.Length; i++)
                {
                    UiNode node;
                    string name = parts[i];
                    string path = string.Join(separator, parts, 0, i + 1);
                    if (!dic.TryGetValue(path, out node))
                    {
                        node = new UiContainerNode(name, UiNodeType.Directory);
                        dic.Add(path, node);
                    }
                    node.Parent = parent;
                    parent = node;
                }
            }

            UiNode[] result = EmptyChilds;
            foreach (IGrouping<UiNode, UiNode> group in dic.Values.GroupBy(n => n.Parent))
            {
                if (group.Key == this)
                {
                    result = group.ToArray();
                    continue;
                }

                UiContainerNode dir = ((UiContainerNode)group.Key);
                dir.SetChilds(group.ToArray());
            }

            foreach (UiNode node in dic.Values)
            {
                if (node.Type != UiNodeType.Directory)
                    continue;

                UiContainerNode container = (UiContainerNode)node;
                container.AbsorbSingleChildContainer();
            }

            return result;
        }

        private static string NormalizeEntryPath(string entryPath)
        {
            if (string.IsNullOrEmpty(entryPath))
                return null;

            string[] parts = entryPath.Replace('\\', '/').Split('/');
            List<string> result = new(parts.Length);

            foreach (string part in parts)
            {
                if (part.Length == 0)
                    continue;

                if (part == ".")
                    continue;

                if (part == "..")
                {
                    if (result.Count > 0)
                        result.RemoveAt(result.Count - 1);
                    continue;
                }

                result.Add(part);
            }

            if (result.Count == 0)
                return null;

            return string.Join("/", result);
        }
    }
}