using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrFileKeysPacker
    {
        private readonly Stream _output;
        private readonly ZtrFileEntry[] _input;
        private readonly ZtrCompressionOptions _options;

        public ZtrFileKeysPacker(Stream output, ZtrFileEntry[] input)
            : this(output, input, null)
        {
        }

        public ZtrFileKeysPacker(Stream output, ZtrFileEntry[] input, ZtrCompressionOptions options)
        {
            _output = output;
            _input = input;
            _options = options ?? ZtrCompressionOptions.Default;
        }

        public unsafe void Pack(ZtrFileHeader header)
        {
            int uncompressedSize = 0;

            ushort index = 0;
            byte[] writeBuff = new byte[4096];
            byte[] codeBuff = new byte[256];

            fixed (byte* writeBuffPtr = &writeBuff[0])
            fixed (byte* codeBuffPtr = &codeBuff[0])
            {
                for (int e = 0; e < _input.Length; e++)
                {
                    ZtrFileEntry entry = _input[e];
                    int count = Encoding.ASCII.GetBytes(entry.Key, 0, entry.Key.Length, codeBuff, 0);
                    codeBuff[count++] = 0;

                    for (int b = 0; b < count; b++)
                    {
                        writeBuffPtr[index++] = codeBuffPtr[b];
                        if (index == writeBuff.Length)
                            WriteBlock(writeBuff, ref index, ref uncompressedSize);
                    }
                }
            }

            if (index > 0)
                WriteBlock(writeBuff, ref index, ref uncompressedSize);

            header.KeysUnpackedSize = uncompressedSize;
        }

        private void WriteBlock(byte[] writeBuff, ref ushort engaged, ref int uncompressedSize)
        {
            uncompressedSize += engaged;

            byte[] chunk = ZtrFileEncoding.CompressChunk(writeBuff, engaged, _options.TieRule);
            _output.Write(chunk, 0, chunk.Length);

            engaged = 0;
        }
    }
}
