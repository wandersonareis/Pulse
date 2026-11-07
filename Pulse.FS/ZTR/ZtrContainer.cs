using System;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public static class ZtrContainer
    {
        public const int HeaderSize = 16;
        public const int RecordSize = 32;
        public const int DataAlignment = 0x80;

        private static readonly byte[] MagicBytes = {0x57, 0x50, 0x44, 0x00};

        public static bool TryDetect(Stream input, string fileName, out ZtrContainerType type)
        {
            type = ZtrContainerType.Loose;

            if (input == null)
                return false;

            long position = SavePosition(input);
            try
            {
                string extension = Path.GetExtension(fileName ?? string.Empty);

                if (extension.Equals(".ztr", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!TryReadHeader(input, out WpdHeader header))
                    return false;

                if (FindZtrEntry(header) == null)
                    return false;

                type = ZtrContainerType.Wpd;
                return true;
            }
            finally
            {
                RestorePosition(input, position);
            }
        }

        public static bool TryDetect(Stream input, out ZtrContainerType type)
        {
            type = ZtrContainerType.Loose;

            if (input == null)
                return false;

            long position = SavePosition(input);
            try
            {
                if (!TryReadHeader(input, out WpdHeader header))
                    return false;

                if (FindZtrEntry(header) == null)
                    return false;

                type = ZtrContainerType.Wpd;
                return true;
            }
            finally
            {
                RestorePosition(input, position);
            }
        }

        public static WpdEntry FindZtrEntry(WpdHeader header)
        {
            if (header?.Entries == null)
                return null;

            foreach (WpdEntry entry in header.Entries)
            {
                if (string.Equals(entry.Extension, "ztr", StringComparison.OrdinalIgnoreCase))
                    return entry;
            }

            return null;
        }

        public static WpdHeader ReadHeader(Stream input)
        {
            if (!TryReadHeader(input, out WpdHeader result))
                throw new InvalidDataException("The container header is invalid.");

            return result;
        }

        private static bool TryReadHeader(Stream input, out WpdHeader result)
        {
            result = null;

            byte[] header = new byte[HeaderSize];
            if (ReadExact(input, header, 0, HeaderSize) != HeaderSize)
                return false;

            if (!HasMagic(header))
                return false;

            int count = ReadBigInt32(header, 4);
            if (count < 0 || count > 0x10000)
                return false;

            byte[] table = new byte[count * RecordSize];
            if (count > 0 && ReadExact(input, table, 0, table.Length) != table.Length)
                return false;

            result = new WpdHeader
            {
                Magic = WpdHeader.MagicNumber,
                Count = count,
                Entries = new WpdEntry[count]
            };

            for (int i = 0; i < count; i++)
            {
                int offset = i * RecordSize;

                result.Entries[i] = new(
                    i,
                    ReadString(table, offset, 16),
                    ReadBigInt32(table, offset + 16),
                    ReadBigInt32(table, offset + 20),
                    ReadString(table, offset + 24, 8));
            }

            return true;
        }

        public static byte[] ReadText(Stream input, out ZtrContainerType type)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (!input.CanSeek)
                throw new NotSupportedException("The stream does not support seeking.");

            long position = SavePosition(input);
            try
            {
                if (HasMagic(input))
                {
                    type = ZtrContainerType.Wpd;

                    input.Position = 0;
                    WpdHeader header = ReadHeader(input);
                    WpdEntry entry = FindZtrEntry(header);
                    if (entry == null)
                        throw new InvalidDataException("The container has no ztr record.");

                    input.Position = entry.Offset;
                    return ReadExactly(input, entry.Length);
                }

                type = ZtrContainerType.Loose;

                // the magic probe has consumed the header, the loose text is the whole stream
                input.Position = position < 0 ? 0 : position;
                return ReadRest(input);
            }
            finally
            {
                RestorePosition(input, position);
            }
        }

        public static void WriteText(Stream output, Stream source, byte[] ztrBytes)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            long position = SavePosition(source);
            try
            {
                if (!source.CanSeek)
                    throw new NotSupportedException("The stream does not support seeking.");

                source.Position = 0;

                byte[] header = ReadExactly(source, HeaderSize);
                if (!HasMagic(header))
                {
                    output.Write(ztrBytes, 0, ztrBytes.Length);
                    return;
                }

                int count = ReadBigInt32(header, 4);
                if (count < 0 || count > 0x10000)
                    throw new InvalidDataException("The container has an invalid record count.");

                byte[] table = count > 0 ? ReadExactly(source, count * RecordSize) : Array.Empty<byte>();

                int ztrIndex = FindZtrEntryIndex(table, count);
                if (ztrIndex < 0)
                    throw new InvalidDataException("The container has no ztr record.");

                int[] offsets = new int[count];
                int[] lengths = new int[count];

                int cursor = Align(HeaderSize + count * RecordSize);
                for (int i = 0; i < count; i++)
                {
                    offsets[i] = cursor;
                    lengths[i] = i == ztrIndex ? ztrBytes.Length : ReadBigInt32(table, i * RecordSize + 20);
                    cursor = Align(cursor + lengths[i]);
                }

                byte[] result = new byte[cursor];
                Buffer.BlockCopy(header, 0, result, 0, HeaderSize);
                Buffer.BlockCopy(table, 0, result, HeaderSize, table.Length);

                for (int i = 0; i < count; i++)
                {
                    WriteBigInt32(result, HeaderSize + i * RecordSize + 16, offsets[i]);
                    WriteBigInt32(result, HeaderSize + i * RecordSize + 20, lengths[i]);
                }

                for (int i = 0; i < count; i++)
                {
                    if (i == ztrIndex)
                    {
                        Buffer.BlockCopy(ztrBytes, 0, result, offsets[i], ztrBytes.Length);
                        continue;
                    }

                    int oldOffset = ReadBigInt32(table, i * RecordSize + 16);
                    source.Position = oldOffset;
                    byte[] data = ReadExactly(source, lengths[i]);
                    Buffer.BlockCopy(data, 0, result, offsets[i], data.Length);
                }

                output.Write(result, 0, result.Length);
            }
            finally
            {
                RestorePosition(source, position);
            }
        }

        private static int FindZtrEntryIndex(byte[] table, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = i * RecordSize + 24;
                int length = 0;
                while (length < 8 && table[index + length] != 0)
                    length++;

                string extension = Encoding.ASCII.GetString(table, index, length);
                if (extension.Equals("ztr", StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private static bool HasMagic(byte[] header)
        {
            for (int i = 0; i < 4; i++)
            {
                if (header[i] != MagicBytes[i])
                    return false;
            }

            return true;
        }

        private static bool HasMagic(Stream input)
        {
            byte[] header = new byte[HeaderSize];
            if (ReadExact(input, header, 0, HeaderSize) < 4)
                return false;

            return HasMagic(header);
        }

        private static byte[] ReadExactly(Stream input, int size)
        {
            byte[] result = new byte[size];
            int readed = ReadExact(input, result, 0, size);
            if (readed != size)
                throw new InvalidDataException("The unexpected end of the stream.");

            return result;
        }

        private static int ReadExact(Stream input, byte[] buffer, int offset, int size)
        {
            int total = 0;
            while (total < size)
            {
                int readed = input.Read(buffer, offset + total, size - total);
                if (readed <= 0)
                    break;

                total += readed;
            }

            return total;
        }

        private static byte[] ReadRest(Stream input)
        {
            using (MemoryStream ms = new())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static long SavePosition(Stream input)
        {
            return input.CanSeek ? input.Position : -1;
        }

        private static void RestorePosition(Stream input, long position)
        {
            if (position < 0 || !input.CanSeek)
                return;

            input.Position = position;
        }

        private static int Align(int value)
        {
            return (value + DataAlignment - 1) / DataAlignment * DataAlignment;
        }

        private static string ReadString(byte[] data, int offset, int size)
        {
            int length = 0;
            while (length < size && data[offset + length] != 0)
                length++;

            return Encoding.ASCII.GetString(data, offset, length);
        }

        private static int ReadBigInt32(byte[] data, int offset)
        {
            return (int)Endian.ToBigUInt32(data, offset);
        }

        private static void WriteBigInt32(byte[] data, int offset, int value)
        {
            data[offset + 0] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }
    }
}
