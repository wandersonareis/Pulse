using System;
using System.IO;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class VtexToDdsWpdEntryExtractor : IWpdEntryExtractor
    {
        public string TargetExtension => "dds";

        public void Extract(WpdEntry entry, Stream output, Lazy<Stream> headers, Lazy<Stream> content, byte[] buff)
        {
            headers.Value.Position = entry.Offset;

            var sectionHeader = headers.Value.ReadContent<SectionHeader>();
            var textureHeader = headers.Value.ReadContent<VtexHeader>();
            headers.Value.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
            var gtex = headers.Value.ReadContent<GtexData>();

            DdsHeader header = DdsHeaderDecoder.FromGtexHeader(gtex.Header);
            DdsHeaderEncoder.ToFileStream(header, output);

            foreach (GtexMipMapLocation mipMap in gtex.MipMapData)
            {
                content.Value.Position = mipMap.Offset;
                content.Value.CopyToStream(output, mipMap.Length, buff);
            }
        }
    }
}