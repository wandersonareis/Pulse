using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiZtrRecordInjector : IDisposable
    {
        private readonly WpdArchiveListing _listing;
        private readonly UiZtrTableLeaf[] _leafs;
        private readonly IUiInjectionSource _source;
        private readonly bool? _compression;
        private bool _injected;

        public UiZtrRecordInjector(WpdArchiveListing listing, UiZtrTableLeaf[] leafs, bool? compression, IUiInjectionSource source)
        {
            _listing = listing;
            _leafs = leafs;
            _compression = compression;
            _source = source;
        }

        public void Dispose()
        {
        }

        public void Inject(UiInjectionManager manager)
        {
            if (_leafs.Length == 0)
                return;

            ArchiveListing parent = _listing.Accessor.Parent;
            ArchiveEntry containerEntry = _listing.Accessor.HeadersEntry;

            string root = _source.ProvideRootDirectory();
            string targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);

            foreach (UiZtrTableLeaf leaf in _leafs)
            {
                WpdEntry entry = leaf.Entry;

                string containerName = Path.GetFileName(PathEx.ChangeMultiDotExtension(leaf.ContainerEntry.Name, ".strings"));
                string targetPath = Path.Combine(targetDirectory, containerName);
                string directoryPath = Path.GetDirectoryName(targetPath);

                if (!_source.DirectoryIsExists(directoryPath) && _source.TryProvideStrings() != null)
                    continue;

                Dictionary<string, string> sourceEntries = TryReadSource(targetPath);
                if (sourceEntries == null)
                    sourceEntries = _source.TryProvideStrings();

                if (sourceEntries == null)
                    continue;

                Inject(entry, sourceEntries, parent, containerEntry);
            }

            if (_injected)
                manager.Enqueue(parent);
        }

        private Dictionary<string, string> TryReadSource(string targetPath)
        {
            using (Stream input = _source.TryOpen(targetPath))
            {
                if (input == null)
                    return null;

                ZtrTextReader reader = new(input, StringsZtrFormatter.Instance);
                return reader.Read(out string _).ToDictionary(e => e.Key, e => e.Value);
            }
        }

        private void Inject(WpdEntry entry, Dictionary<string, string> sourceEntries, ArchiveListing parent, ArchiveEntry containerEntry)
        {
            byte[] container = UiTextUnitExtractor.ReadAllBytes(parent.Accessor.ExtractBinary(containerEntry));

            byte[] payload;
            ZtrFileType type;
            ZtrFileEntry[] targetEntries;

            using (MemoryStream source = new(container))
            {
                byte[] text = ZtrContainer.ReadText(source, out ZtrContainerType _);

                using (MemoryStream input = new(text))
                {
                    ZtrFileUnpacker unpacker = new(input, InteractionService.TextEncoding.Provide().Encoding);
                    targetEntries = unpacker.Unpack();
                    type = unpacker.Type;

                    if (type != ZtrFileType.BigEndianCompressedDictionary && type != ZtrFileType.LittleEndianUncompressedPair)
                        type = ZtrFileType.LittleEndianUncompressedDictionary;
                }

                StringsToZtrArchiveEntryInjector.MergeEntries(sourceEntries, targetEntries);

                byte[] packed;
                using (MemoryStream output = new())
                {
                    ZtrFilePacker packer = new(output, InteractionService.TextEncoding.Provide().Encoding, type);
                    packer.Pack(targetEntries);
                    packed = output.ToArray();
                }

                using (MemoryStream output = new())
                using (MemoryStream original = new(container))
                {
                    ZtrContainer.WriteText(output, original, packed);
                    payload = output.ToArray();
                }
            }

            using (Stream destination = OpenOutputStream(containerEntry, payload.Length))
                destination.Write(payload, 0, payload.Length);

            _injected = true;
        }

        private Stream OpenOutputStream(ArchiveEntry entry, int size)
        {
            MemoryStream ms = new(size > 0 ? size : 1024);

            DisposableStream writer = new(ms);
            writer.BeforeDispose.Add(new DisposableAction(() => _listing.Accessor.Parent.Accessor.OnWritingCompleted(entry, ms, _compression)));

            return writer;
        }
    }
}
