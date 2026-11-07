using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileEncoding
    {
        public readonly int BlockSize;
        public readonly byte[][] Encoding;

        private ZtrFileEncoding(int blockSize, byte[][] encoding)
        {
            BlockSize = blockSize;
            Encoding = encoding;
        }

        public static unsafe ZtrFileEncoding ReadFromStream(Stream input)
        {
            int blockSize;
            byte[] buff = input.EnsureRead(4);
            fixed (byte* b = &buff[0])
                blockSize = Endian.ToBigInt32(b);

            byte[] values = new byte[blockSize / 3];
            byte[,] encoding = new byte[256, 2];
            if (blockSize > 0)
            {
                buff = input.EnsureRead(blockSize);
                fixed (byte* b = &buff[0])
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        byte value = *(b + i * 3);
                        encoding[value, 0] = *(b + i * 3 + 1);
                        encoding[value, 1] = *(b + i * 3 + 2);
                        values[i] = value;
                    }
                }
            }

            List<byte>[] lists = new List<byte>[256];
            for (int i = 0; i < 256; i++)
            {
                List<byte> list = lists[i] = new(16);
                DecodeByte(values, encoding, (byte)i, list);
            }

            byte[][] result = new byte[256][];
            for (int i = 0; i < 256; i++)
                result[i] = lists[i].ToArray();

            return new(blockSize, result);
        }

        public unsafe void WriteToStream(Stream output)
        {
            BinaryWriter bw = new(output);
            bw.WriteBig(BlockSize);

            if (BlockSize > 0)
                throw new NotImplementedException();
        }

        private static void DecodeByte(byte[] knownValues, byte[,] encoding, byte value, List<byte> list)
        {
            if (Array.IndexOf(knownValues, value) < 0)
            {
                list.Add(value);
            }
            else
            {
                DecodeByte(knownValues, encoding, encoding[value, 0], list);
                DecodeByte(knownValues, encoding, encoding[value, 1], list);
            }
        }

        public static byte[] CompressZtrContent([In, Out] byte[] data, int dataIndex, ref ushort dataSize)
        {
            return CompressZtrContent(data, dataIndex, ref dataSize, new ushort[0,0], 0, 0);
        }

        // WARNING BUG: Don't use it for text packing or fix BlockOffset first!
        // It should be contains offset if the begining of the entry was mixed with a previous tail.
        public static unsafe byte[] CompressZtrContent([In, Out] byte[] data, int dataIndex, ref ushort dataSize, [In, Out] ushort[,] innerOffsets, int innerIndex, int innerCount)
        {
            ushort maxValue = 0;
            int[] singleCounter = new int[byte.MaxValue + 1];
            int[] pairCounter = new int[ushort.MaxValue + 1];
            byte[,] encoding = new byte[256, 2];

            List<byte> unused = new(256);
            List<byte> used = new(256);
            fixed (byte* dataPtr = &data[dataIndex])
            fixed (int* pairCounterPtr = pairCounter)
            fixed (int* singleCounterPtr = singleCounter)
            {
                int index;
                for (index = 0; index < 256; index++)
                {
                    bool finded = false;
                    for (int b = 0; b < dataSize; b++)
                    {
                        byte val = dataPtr[b];
                        if (index == val)
                        {
                            singleCounterPtr[val]++;
                            finded = true;
                        }
                    }
                    if (!finded)
                        unused.Add((byte)index);
                }

                for (int unusedIndex = 0; unusedIndex < unused.Count; unusedIndex++)
                {
                    byte value = unused[unusedIndex];
                    int maxCounter = 0;

                    for (int i = 0; i < 256; i++)
                    {
                        int count = singleCounterPtr[i];
                        if (count > maxCounter)
                        {
                            index = i;
                            maxCounter = count;
                        }
                    }

                    if (maxCounter == 0)
                        break;

                    singleCounterPtr[index] = 0;
                    maxCounter = 0;

                    Array.Clear(pairCounter, 0, pairCounter.Length);

                    byte left, right;
                    for (int b = 0; b < dataSize - 1; b++)
                    {
                        left = dataPtr[b];
                        right = dataPtr[b + 1];
                        if (left != index && right != index)
                            continue;

                        ushort pair = (ushort)((left << 8) | right);
                        int count = ++pairCounterPtr[pair];
                        if (count > maxCounter)
                        {
                            maxCounter = count;
                            maxValue = pair;
                        }
                    }

                    if (maxCounter < 2)
                    {
                        unusedIndex--;
                        continue;
                    }

                    left = (byte)((maxValue >> 8) & 0xFF);
                    right = (byte)(maxValue & 0xFF);
                    encoding[value, 0] = left;
                    encoding[value, 1] = right;
                    used.Add(value);

                    for (int b = 0; b < dataSize - 1; b++)
                    {
                        if (dataPtr[b] != left || dataPtr[b + 1] != right)
                            continue;

                        // BlockOffset, (bug -_-)!
                        for (int i = 0; i < innerCount - 1; i++)
                        {
                            ushort offset = innerOffsets[i + innerIndex, 0];
                            if (offset > b + 1)
                                break;

                            if (offset == b + 1)
                            {
                                innerOffsets[i + innerIndex + 1, 1]++;
                                break;
                            }
                        }

                        dataPtr[b] = value;

                        for (int m = b + 1; m < dataSize - 1; m++)
                            dataPtr[m] = dataPtr[m + 1];

                        // PackedOffset
                        for (int i = 0; i < innerCount; i++)
                        {
                            if (innerOffsets[i + innerIndex, 0] > b)
                                innerOffsets[i + innerIndex, 0]--;
                        }

                        dataSize--;
                    }
                }
            }

            int blockSize = used.Count * 3;
            byte[] result = new byte[blockSize + 4];
            fixed (byte* arrayPtr = &result[0])
            {
                BinaryWriterExm.WriteBig(arrayPtr, blockSize);
                byte* ptr = arrayPtr + 4;

                for (int index = 0; index < used.Count; index++)
                {
                    byte value = used[index];
                    ptr[0] = value;
                    ptr[1] = encoding[value, 0];
                    ptr[2] = encoding[value, 1];
                    ptr += 3;
                }
            }

            return result;
        }

        public static unsafe byte[] FakeCompression([In, Out] byte[] data, int dataIndex, ref ushort dataSize, [In, Out] ushort[,] innerOffsets, int innerIndex, int innerCount)
        {
            const int blockSize = 0;
            byte[] result = new byte[blockSize + 4];
            fixed (byte* arrayPtr = &result[0])
                BinaryWriterExm.WriteBig(arrayPtr, blockSize);

            return result;
        }

        /// <summary>
        /// The size of the uncompressed data of a single ZTR chunk.
        /// </summary>
        public const int MaxChunkData = 4096;

        /// <summary>
        /// The page index that is never used as a dictionary pointer.
        /// </summary>
        private const byte PageIndexSkip = 8;

        /// <summary>
        /// Compresses the chunk with the real ZTR compression: the most frequent
        /// pairs of bytes are replaced with the pointers to the dictionary pages.
        /// <para>
        /// The result is the <c>[u32 big endian dictionary size][dictionary triples][data]</c>
        /// structure, where every triple is the <c>(page index, byte 1, byte 2)</c> tuple.
        /// </para>
        /// </summary>
        /// <param name="data">The uncompressed data of the chunk.</param>
        /// <param name="tie">The rule used to resolve the ties between the equally frequent pairs.</param>
        public static byte[] CompressChunk(byte[] data, ZtrCompressionTieRule tie)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return CompressChunk(data, data.Length, tie);
        }

        /// <summary>
        /// Compresses the first <paramref name="count"/> bytes of the chunk with the real
        /// ZTR compression.
        /// </summary>
        public static byte[] CompressChunk(byte[] data, int count, ZtrCompressionTieRule tie)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (count < 0 || count > data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            List<byte> pages = GetFreePageNumbers(data, count);
            List<byte> dictionary = new();

            byte[] current = new byte[count];
            Buffer.BlockCopy(data, 0, current, 0, count);

            for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
            {
                (byte byte1, byte byte2, int occurrences) = GetLargestOccurringBytes(current, tie);
                if (occurrences < 4)
                    break;

                byte page = pages[pageIndex];
                dictionary.Add(page);
                dictionary.Add(byte1);
                dictionary.Add(byte2);

                byte[] processed = new byte[current.Length];
                int position = 0;

                for (int i = 0; i < current.Length; i++)
                {
                    if (i == current.Length - 1)
                    {
                        processed[position++] = current[i];
                    }
                    else if (current[i] == byte1 && current[i + 1] == byte2)
                    {
                        processed[position++] = page;
                        i++;
                    }
                    else
                    {
                        processed[position++] = current[i];
                    }
                }

                if (position != processed.Length)
                    Array.Resize(ref processed, position);

                current = processed;
            }

            byte[] result = new byte[4 + dictionary.Count + current.Length];

            result[0] = (byte)(dictionary.Count >> 24);
            result[1] = (byte)(dictionary.Count >> 16);
            result[2] = (byte)(dictionary.Count >> 8);
            result[3] = (byte)dictionary.Count;

            Buffer.BlockCopy(dictionary.ToArray(), 0, result, 4, dictionary.Count);
            Buffer.BlockCopy(current, 0, result, 4 + dictionary.Count, current.Length);

            return result;
        }

        /// <summary>
        /// Returns the byte values that can be used as the dictionary page indexes:
        /// the values that do not occur in the data, except the reserved one.
        /// </summary>
        private static List<byte> GetFreePageNumbers(byte[] data, int count)
        {
            bool[] present = new bool[256];
            for (int i = 0; i < count; i++)
                present[data[i]] = true;

            List<byte> result = new(256);
            for (int i = 0; i < 256; i++)
            {
                if (i == PageIndexSkip)
                    continue;

                if (!present[i])
                    result.Add((byte)i);
            }

            return result;
        }

        /// <summary>
        /// Searches the most frequent pair of bytes. The pairs are counted from the first
        /// occurrence of the pair, the further non overlapping matches are looked up from
        /// the second byte of the previous match.
        /// </summary>
        private static (byte, byte, int) GetLargestOccurringBytes(byte[] data, ZtrCompressionTieRule tie)
        {
            int length = data.Length;
            if (length < 2)
                return (0, 0, 0);

            Dictionary<int, List<int>> occurrences = new();
            List<int> order = new();

            for (int i = 0; i < length - 1; i++)
            {
                int key = (data[i] << 8) | data[i + 1];

                if (!occurrences.TryGetValue(key, out List<int> positions))
                {
                    positions = new List<int>();
                    occurrences.Add(key, positions);
                    order.Add(key);
                }

                positions.Add(i);
            }

            int bestKey = 0;
            int bestCount = 0;
            int bestLastOccurrence = int.MaxValue;

            foreach (int key in order)
            {
                List<int> positions = occurrences[key];

                int currentCount = 1;
                int next = positions[0] + 2;

                for (int i = 1; i < positions.Count; i++)
                {
                    if (positions[i] < next)
                        continue;

                    currentCount++;
                    next = positions[i] + 2;
                }

                int lastOccurrence = positions[positions.Count - 1];

                bool better;
                if (tie == ZtrCompressionTieRule.TieFirst)
                    better = currentCount > bestCount;
                else
                    better = currentCount > bestCount || (currentCount == bestCount && lastOccurrence < bestLastOccurrence);

                if (!better)
                    continue;

                bestKey = key;
                bestCount = currentCount;
                bestLastOccurrence = lastOccurrence;
            }

            if (bestCount == 0)
                return (0, 0, 0);

            return ((byte)(bestKey >> 8), (byte)bestKey, bestCount);
        }
    }
}