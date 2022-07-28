using System.IO;
using System.Linq;
using System.Windows;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using Yusnaan.Common;

namespace Yusnaan.Formats.imgb
{
    public class XfvInject
    {
        private readonly string? _filename;
        private readonly WpdEntry? _entry;
        private readonly Stream _input;
        private readonly Stream _header;
        private readonly Stream _contents;
        private readonly byte[]? _buff;

        public XfvInject(string? name, Stream input, Stream headers, Stream contents)
        {
            _filename = name;
            _input = input;
            _header = headers;
            _contents = contents;
        }

        public XfvInject(WpdEntry? entry, Stream input, Stream headers, Stream contents, byte[]? buff)
        {
            _entry = entry;
            _input = input;
            _header = headers;
            _contents = contents;
            _buff = buff;
        }

        public void InjectAll()
        {
            var sourceSize = (int)_input.Length;
            _header.Position = _entry!.Offset;

            var sectionHeader = _header.ReadContent<SectionHeader>();
            var textureHeader = _header.ReadContent<VtexHeader>();

            var unknownData = new byte[textureHeader.GtexOffset - VtexHeader.Size];
            _header.Read(unknownData, 0, unknownData.Length);

            var data = _header.ReadContent<GtexData>();

            if (data.MipMapData.Length != 1)
            {
                throw new MipMapException($"{_entry.NameWithoutExtension} with mipmap! Skipped this file.");
            }

            DdsHeader ddsHeader = DdsHeaderDecoder.FromFileStream(_input);
            DdsHeaderEncoder.ToGtexHeader(ddsHeader, data.Header);

            GtexMipMapLocation mipMapLocation = data.MipMapData[0];
            int dataSize = sourceSize - 128;
            if (dataSize <= mipMapLocation.Length)
            {
                _contents.Seek(mipMapLocation.Offset, SeekOrigin.Begin);
            }
            else
            {
                _contents.Seek(0, SeekOrigin.End);
                mipMapLocation.Offset = (int)_contents.Position;
            }

            _input.CopyToStream(_contents, dataSize, _buff);
            mipMapLocation.Length = dataSize;
        }
        public void Inject()
        {
            var wdbHeader = _header.ReadContent<WdbHeader>();
            var buff = new byte[32 * 1024];

            foreach (WpdEntry entry in wdbHeader.Entries.Where(e => e.NameWithoutExtension == _filename))
            {
                var sourceSize = (int)_input.Length;
                _header.Position = entry.Offset;

                var sectionHeader = _header.ReadContent<SectionHeader>();
                var textureHeader = _header.ReadContent<VtexHeader>();

                var unknownData = new byte[textureHeader.GtexOffset - VtexHeader.Size];
                _header.Read(unknownData, 0, unknownData.Length);

                var data = _header.ReadContent<GtexData>();

                if (data.MipMapData.Length != 1)
                {
                    MessageBox.Show($"{entry.NameWithoutExtension} with mipmap! Skipped this file.");
                    continue;
                }

                DdsHeader ddsHeader = DdsHeaderDecoder.FromFileStream(_input);
                DdsHeaderEncoder.ToGtexHeader(ddsHeader, data.Header);

                GtexMipMapLocation mipMapLocation = data.MipMapData[0];
                int dataSize = sourceSize - 128;
                if (dataSize <= mipMapLocation.Length)
                {
                    _contents.Seek(mipMapLocation.Offset, SeekOrigin.Begin);
                }
                else
                {
                    _contents.Seek(0, SeekOrigin.End);
                    mipMapLocation.Offset = (int)_contents.Position;
                }

                _input.CopyToStream(_contents, dataSize, buff);
                mipMapLocation.Length = dataSize;
            }
        }
    }
}
