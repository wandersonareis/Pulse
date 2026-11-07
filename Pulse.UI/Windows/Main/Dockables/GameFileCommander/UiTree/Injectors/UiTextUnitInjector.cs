using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiTextUnitInjector
    {
        private readonly ArchiveListing _listing;
        private readonly UiTextUnitLeaf[] _leafs;
        private readonly IUiInjectionSource _source;
        private readonly bool? _compression;
        private bool _injected;

        public UiTextUnitInjector(ArchiveListing listing, UiTextUnitLeaf[] leafs, bool? compression, IUiInjectionSource source)
        {
            _listing = listing;
            _leafs = leafs;
            _compression = compression;
            _source = source;
        }

        public void Inject(UiInjectionManager manager)
        {
            if (_leafs.Length == 0)
                return;

            string root = _source.ProvideRootDirectory();

            foreach (UiTextUnitLeaf leaf in _leafs)
            {
                if (!leaf.IsEditable)
                    continue;

                ArchiveEntry entry = leaf.Entry;
                string sourcePath = Path.Combine(root, PathEx.ChangeMultiDotExtension(entry.Name, ".strings"));
                string directoryPath = Path.GetDirectoryName(sourcePath);

                if (!_source.DirectoryIsExists(directoryPath) && _source.TryProvideStrings() == null)
                    continue;

                Dictionary<string, string> sourceEntries = TryReadSource(sourcePath);
                if (sourceEntries == null)
                    sourceEntries = _source.TryProvideStrings();

                if (sourceEntries == null)
                    continue;

                Inject(entry, sourceEntries);
            }

            if (_injected)
                manager.Enqueue(_listing);
        }

        private Dictionary<string, string> TryReadSource(string sourcePath)
        {
            using (Stream input = _source.TryOpen(sourcePath))
            {
                if (input == null)
                    return null;

                ZtrTextReader reader = new(input, StringsZtrFormatter.Instance);
                return reader.Read(out string _).ToDictionary(e => e.Key, e => e.Value);
            }
        }

        private void Inject(ArchiveEntry entry, Dictionary<string, string> sourceEntries)
        {
            byte[] container = UiTextUnitExtractor.ReadAllBytes(ExtractEntry(entry));

            using (MemoryStream source = new(container))
            {
                byte[] text = ZtrContainer.ReadText(source, out ZtrContainerType _);

                ZtrFileEntry[] targetEntries;
                ZtrFileType type;
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

                using (Stream destination = OpenOutputStream(entry))
                using (MemoryStream original = new(container))
                    ZtrContainer.WriteText(destination, original, packed);
            }

            _injected = true;
        }

        private Stream ExtractEntry(ArchiveEntry entry)
        {
            return _listing.Accessor.ExtractBinary(entry);
        }

        private Stream OpenOutputStream(ArchiveEntry entry)
        {
            MemoryStream ms = new((int)(entry.UncompressedSize * 1.3));

            DisposableStream writer = new(ms);
            writer.BeforeDispose.Add(new DisposableAction(() => _listing.Accessor.OnWritingCompleted(entry, ms, _compression)));

            return writer;
        }
    }
}
