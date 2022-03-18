using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class ZtrToStringsArchiveEntryExtractor : IArchiveEntryExtractor, IWpdEntryExtractor
    {
        public string TargetExtension => ".strings";

        public void Extract(ArchiveEntry entry, StreamSequence output, Stream input, byte[] buff)
        {
            int size = (int)entry.UncompressedSize;
            if (size == 0)
                return;

            ZtrFileUnpacker unpacker = new ZtrFileUnpacker(input, InteractionService.TextEncoding.Provide().Encoding);
            ZtrFileEntry[] entries = unpacker.Unpack();

            ZtrTextWriter writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
            writer.Write(entry.Name, entries);
        }

        public void Extract(WpdEntry entry, Stream output, Lazy<Stream> headers, Lazy<Stream> content, byte[] buff)
        {
            headers.Value.Position = entry.Offset;

            ZtrFileUnpacker unpacker = new ZtrFileUnpacker(headers.Value, InteractionService.TextEncoding.Provide().Encoding);
            ZtrFileEntry[] entries = unpacker.Unpack();

            ZtrTextWriter writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
            writer.Write(entry.Name, entries);
        }
    }
}