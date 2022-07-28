using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFilePacker
    {
        private readonly FFXIIITextEncoding _encoding;
        private readonly Stream _output;
        private readonly BinaryWriter _bw;
        private readonly ZtrFileType? _type;

        public ZtrFilePacker(Stream output, FFXIIITextEncoding encoding, ZtrFileType? type)
        {
            _encoding = encoding;
            _output = output;
            _bw = new(_output);
            _type = type;
        }

        public void Pack(ZtrFileEntry[] entries)
        {
            if (entries.Length == 0 || _type == ZtrFileType.BigEndianCompressedDictionary)
                PackBigEndianCompressedDictionary(entries);
            else if (entries.Length == 1)
                PackLittleEndianUncompressedPair(entries[0]);
            else
                PackLittleEndianUncompressedDictionary(entries);
        }

        private void PackLittleEndianUncompressedDictionary(ZtrFileEntry[] entries)
        {
            int count = entries.Length;
            var keys = new byte[count][];
            var values = new byte[count][];
            var offsets = new int[count * 2];

            var index = 0;
            offsets[0] = 4 + count * 2 * 4;
            for (var i = 0; i < count; i++)
            {
                ZtrFileEntry entry = entries[i];
                keys[i] = Encoding.ASCII.GetBytes(entry.Key);
                values[i] = (entry.IsAnimatedText ? FFXIIITextEncodingFactory.DefaultEuroEncoding.Value : _encoding).GetBytes(entry.Value);
                var teste = Encoding.UTF8.GetString(values[i]);
                offsets[index + 1] = offsets[index++] + keys[i].Length + 1;
                if (index + 1 < offsets.Length)
                    offsets[index + 1] = offsets[index++] + values[i].Length + 2;
            }

            _bw.Write(count);
            for (var i = 0; i < count * 2; i++)
                _bw.Write(offsets[i]);

            for (var i = 0; i < count; i++)
            {
                _bw.Write(keys[i], 0, keys[i].Length);
                _bw.Write((byte)0);
                _bw.Write(values[i], 0, values[i].Length);
                _bw.Write((short)0);
            }
        }

        private void PackLittleEndianUncompressedPair(ZtrFileEntry entry)
        {
            _bw.Write((int)ZtrFileType.LittleEndianUncompressedPair);

            byte[] key = Encoding.ASCII.GetBytes(entry.Key);
            byte[] value = (entry.IsAnimatedText ? FFXIIITextEncodingFactory.DefaultEuroEncoding.Value : _encoding).GetBytes(entry.Value);

            _bw.Write(12);
            _bw.Write(12 + key.Length + 1);

            _bw.Write(key);
            _bw.Write((byte)0);
            _bw.Write(value);
            _bw.Write((short)0);
        }

        private void PackBigEndianCompressedDictionary(ZtrFileEntry[] entries)
        {
            _bw.Write((int)ZtrFileType.BigEndianCompressedDictionary);

            var header = new ZtrFileHeader
            {
                Version = 1,
                Count = entries.Length
            };

            using (var ms = new MemoryStream(32 * 1024))
            {
                var keyPacker = new ZtrFileKeysPacker(ms, entries);
                keyPacker.Pack(header);

                var textPacker = new ZtrFileTextPacker(ms, entries, _encoding);
                textPacker.Pack(header);

                header.WriteToStream(_output);
                ms.Position = 0;
                ms.CopyTo(_output);
            }
        }
    }
}