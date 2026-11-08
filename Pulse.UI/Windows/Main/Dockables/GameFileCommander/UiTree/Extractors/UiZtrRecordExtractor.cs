using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiZtrRecordExtractor : IDisposable
    {
        private readonly WpdArchiveListing _listing;
        private readonly UiZtrTableLeaf[] _leafs;
        private readonly IUiExtractionTarget _target;
        private readonly Lazy<Stream> _headers;

        public UiZtrRecordExtractor(WpdArchiveListing listing, UiZtrTableLeaf[] leafs, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _target = target;
            _headers = new(_listing.Accessor.ExtractHeaders);
        }

        public void Dispose()
        {
            _headers.NullSafeDispose();
        }

        public void Extract()
        {
            if (_leafs.Length == 0)
                return;

            string root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();
            string targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            _target.CreateDirectory(targetDirectory);

            Stream headers = _headers.Value;
            byte[] buff = new byte[32 * 1024];

            foreach (UiZtrTableLeaf leaf in _leafs)
            {
                WpdEntry entry = leaf.Entry;

                string containerName = Path.GetFileName(PathEx.ChangeMultiDotExtension(leaf.ContainerEntry.Name, ".strings"));
                string targetPath = Path.Combine(targetDirectory, containerName);
                using (StreamSequence output = _target.Create(targetPath))
                {
                    headers.Position = entry.Offset;

                    ZtrFileUnpacker unpacker = new(headers, InteractionService.TextEncoding.Provide().Encoding);
                    ZtrFileEntry[] entries = unpacker.Unpack();

                    ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
                    writer.Write(leaf.ContainerEntry.Name, entries);
                }
            }
        }
    }
}
