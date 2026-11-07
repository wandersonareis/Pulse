using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileTextPacker
    {
        private readonly Stream _output;
        private readonly ZtrFileEntry[] _input;
        private readonly FFXIIITextEncoding _encoding;
        private readonly ZtrCompressionOptions _options;

        public ZtrFileTextPacker(Stream output, ZtrFileEntry[] input, FFXIIITextEncoding encoding)
            : this(output, input, encoding, null)
        {
        }

        public ZtrFileTextPacker(Stream output, ZtrFileEntry[] input, FFXIIITextEncoding encoding, ZtrCompressionOptions options)
        {
            _output = output;
            _input = input;
            _encoding = encoding;
            _options = options ?? ZtrCompressionOptions.Default;
        }

        public void Pack(ZtrFileHeader header)
        {
            byte[] blob = BuildTextBlob();
            int[] lineStarts = BuildLineStarts(blob);

            List<byte[]> chunks = new();
            List<ZtrFileHeaderLineInfo> lines = new();

            if (_options.Mode == ZtrCompressionMode.Uncompressed)
                PackUncompressed(blob, lineStarts, chunks, lines);
            else
                PackCompressed(blob, lineStarts, chunks, lines);

            List<int> blockTable = new(chunks.Count + 1) {0};
            int offset = 0;

            foreach (byte[] chunk in chunks)
            {
                _output.Write(chunk, 0, chunk.Length);
                offset += chunk.Length;
                blockTable.Add(offset);
            }

            header.TextBlocksCount = blockTable.Count;
            header.TextBlockTable = blockTable.ToArray();
            header.TextLinesTable = lines.ToArray();
        }

        /// <summary>
        /// Builds the blob of the text: the encoded bytes of every line terminated
        /// with the two zero bytes.
        /// </summary>
        private byte[] BuildTextBlob()
        {
            using (MemoryStream ms = new(32 * 1024))
            {
                byte[] codeBuff = new byte[64 * 1024];

                foreach (ZtrFileEntry entry in _input)
                {
                    int count = _encoding.GetBytes(entry.Value, 0, entry.Value.Length, codeBuff, 0);
                    ms.Write(codeBuff, 0, count);
                    ms.WriteByte(0);
                    ms.WriteByte(0);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Returns the offset of the beginning of every line of the blob.
        /// </summary>
        private static int[] BuildLineStarts(byte[] blob)
        {
            List<int> result = new();

            int position = 0;
            while (position < blob.Length)
            {
                result.Add(position);

                int index = -1;
                for (int i = position; i < blob.Length - 1; i++)
                {
                    if (blob[i] == 0 && blob[i + 1] == 0)
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                    break;

                position = index + 2;
            }

            return result.ToArray();
        }

        private void PackCompressed(byte[] blob, int[] lineStarts, List<byte[]> chunks, List<ZtrFileHeaderLineInfo> lines)
        {
            int position = 0;
            while (position < blob.Length)
            {
                int size = Math.Min(ZtrFileEncoding.MaxChunkData, blob.Length - position);
                byte[] group = new byte[size];
                Buffer.BlockCopy(blob, position, group, 0, size);

                chunks.Add(ZtrFileEncoding.CompressChunk(group, _options.TieRule));
                position += size;
            }

            // The offsets of the lines are resolved in the expanded data, so the
            // dictionary of every chunk is parsed to build the map of the expanded
            // offsets of its bytes.
            int[] bases = new int[chunks.Count];
            List<int>[] maps = new List<int>[chunks.Count];

            int total = 0;
            for (int i = 0; i < chunks.Count; i++)
            {
                byte[] chunk = chunks[i];
                Dictionary<byte, byte[]> pages = ParseDictionary(chunk, out int stringsPosition);

                List<int> map = new(chunk.Length - stringsPosition);
                int expanded = 0;

                for (int c = stringsPosition; c < chunk.Length; c++)
                {
                    map.Add(expanded);

                    if (pages.TryGetValue(chunk[c], out byte[] expansion))
                        expanded += expansion.Length;
                    else
                        expanded++;
                }

                bases[i] = total;
                maps[i] = map;
                total += expanded;
            }

            foreach (int start in lineStarts)
            {
                int chunkIndex = BisectRight(bases, start) - 1;
                if (chunkIndex < 0)
                    throw new InvalidDataException($"The line at the offset {start} does not belong to any chunk.");

                int relative = start - bases[chunkIndex];
                List<int> map = maps[chunkIndex];

                int position2 = BisectRight(map, relative) - 1;
                if (position2 < 0)
                    throw new InvalidDataException($"The line at the offset {start} does not belong to any chunk.");

                int innerOffset = relative - map[position2];

                lines.Add(new ZtrFileHeaderLineInfo
                {
                    Block = (byte)chunkIndex,
                    BlockOffset = checked((byte)innerOffset),
                    PackedOffset = checked((ushort)position2)
                });
            }
        }

        private static void PackUncompressed(byte[] blob, int[] lineStarts, List<byte[]> chunks, List<ZtrFileHeaderLineInfo> lines)
        {
            // The chunks of the uncompressed mode contain no dictionary at all and
            // keep the raw data. The position of the line is evaluated before the
            // cut of the chunk, so the line that ends exactly on the boundary of the
            // chunk points to the position right after the end of the chunk.
            byte[] buffer = new byte[4 + ZtrFileEncoding.MaxChunkData];
            int chunkIndex = 0;
            int copyCounter = 0;
            byte previousByte = 0xFF;

            lines.Add(new ZtrFileHeaderLineInfo());

            for (int i = 0; i < blob.Length; i++)
            {
                byte value = blob[i];
                copyCounter++;
                buffer[4 + copyCounter - 1] = value;

                if (value == 0 && previousByte == 0)
                {
                    int index = lines.Count;
                    if (index < lineStarts.Length)
                    {
                        lines.Add(new ZtrFileHeaderLineInfo
                        {
                            Block = (byte)chunkIndex,
                            PackedOffset = checked((ushort)copyCounter)
                        });
                    }

                    previousByte = 0xFF;
                }
                else
                {
                    previousByte = value;
                }

                if (copyCounter != ZtrFileEncoding.MaxChunkData)
                    continue;

                chunks.Add(TrimChunk(buffer, copyCounter));
                copyCounter = 0;
                chunkIndex++;
            }

            chunks.Add(TrimChunk(buffer, copyCounter));
        }

        private static byte[] TrimChunk(byte[] buffer, int size)
        {
            byte[] result = new byte[4 + size];
            Buffer.BlockCopy(buffer, 0, result, 0, 4 + size);
            return result;
        }

        /// <summary>
        /// Parses the dictionary of the chunk and resolves the pages into the
        /// expanded bytes.
        /// </summary>
        private static Dictionary<byte, byte[]> ParseDictionary(byte[] chunk, out int stringsPosition)
        {
            int dictionarySize = (chunk[0] << 24) | (chunk[1] << 16) | (chunk[2] << 8) | chunk[3];
            int basePosition = 4;

            List<byte> indexes = new(dictionarySize / 3);
            List<byte> first = new(dictionarySize / 3);
            List<byte> second = new(dictionarySize / 3);

            for (int i = 0; i < dictionarySize / 3; i++)
            {
                indexes.Add(chunk[basePosition + i * 3]);
                first.Add(chunk[basePosition + i * 3 + 1]);
                second.Add(chunk[basePosition + i * 3 + 2]);
            }

            Dictionary<byte, byte[]> result = new();

            for (int i = 0; i < indexes.Count; i++)
            {
                byte[] left = result.TryGetValue(first[i], out byte[] leftValue) ? leftValue : new[] {first[i]};
                byte[] right = result.TryGetValue(second[i], out byte[] rightValue) ? rightValue : new[] {second[i]};

                byte[] expansion = new byte[left.Length + right.Length];
                Buffer.BlockCopy(left, 0, expansion, 0, left.Length);
                Buffer.BlockCopy(right, 0, expansion, left.Length, right.Length);

                result[indexes[i]] = expansion;
            }

            stringsPosition = basePosition + dictionarySize;
            return result;
        }

        /// <summary>
        /// Returns the index of the first element of the sorted array that is greater
        /// than the specified value.
        /// </summary>
        private static int BisectRight(int[] array, int value)
        {
            int low = 0;
            int high = array.Length;

            while (low < high)
            {
                int middle = (low + high) / 2;

                if (array[middle] <= value)
                    low = middle + 1;
                else
                    high = middle;
            }

            return low;
        }

        private static int BisectRight(List<int> array, int value)
        {
            int low = 0;
            int high = array.Count;

            while (low < high)
            {
                int middle = (low + high) / 2;

                if (array[middle] <= value)
                    low = middle + 1;
                else
                    high = middle;
            }

            return low;
        }
    }
}
