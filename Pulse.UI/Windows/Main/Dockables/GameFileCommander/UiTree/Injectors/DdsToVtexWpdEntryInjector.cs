using System;
using System.IO;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class DdsToVtexWpdEntryInjector : IWpdEntryInjector
    {
        public string SourceExtension => "dds";

        public void Inject(WpdEntry entry, Stream input, Lazy<Stream> headers, Lazy<Stream> content, byte[] buff)
        {
            var sourceSize = (int)input.Length;
            headers.Value.Position = entry.Offset;

            var sectionHeader = headers.Value.ReadContent<SectionHeader>();
            var textureHeader = headers.Value.ReadContent<VtexHeader>();

            var unknownData = new byte[textureHeader.GtexOffset - VtexHeader.Size];
            headers.Value.Read(unknownData, 0, unknownData.Length);

            var data = headers.Value.ReadContent<GtexData>();

            if (data.MipMapData.Length != 1)
                throw new NotImplementedException();

            DdsHeader ddsHeader = DdsHeaderDecoder.FromFileStream(input);
            DdsHeaderEncoder.ToGtexHeader(ddsHeader, data.Header);

            GtexMipMapLocation mipMapLocation = data.MipMapData[0];
            int dataSize = sourceSize - 128;
            if (dataSize <= mipMapLocation.Length)
            {
                content.Value.Seek(mipMapLocation.Offset, SeekOrigin.Begin);
            }
            else
            {
                content.Value.Seek(0, SeekOrigin.End);
                mipMapLocation.Offset = (int)content.Value.Position;
            }

            input.CopyToStream(content.Value, dataSize, buff);
            mipMapLocation.Length = dataSize;

            using (MemoryStream ms = new(180))
            {
                sectionHeader.WriteToStream(ms);
                textureHeader.WriteToStream(ms);

                ms.Write(unknownData, 0, unknownData.Length);
                data.WriteToStream(ms);

                ms.SetPosition(0);

                var defaultInjector = new DefaultWpdEntryInjector();
                defaultInjector.Inject(entry, ms, headers, content, buff);
            }
        }
    }
}