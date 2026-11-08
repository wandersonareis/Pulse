using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pulse.Core;
using Pulse.FS;
using Xunit;

namespace Pulse.Tests
{
    public sealed class ZtrTests
    {
        private static FFXIIITextEncoding Encoding => FFXIIITextEncodingFactory.DefaultEuroEncoding.Value;

        [Fact]
        public void Unpack_LooseZtr_ReturnsEntries()
        {
            if (!TestPaths.Exists(TestPaths.LooseZtr))
                return;

            ZtrFileEntry[] entries;
            ZtrFileType type;

            using (FileStream input = File.OpenRead(TestPaths.LooseZtr))
            {
                ZtrFileUnpacker unpacker = new(input, Encoding);
                entries = unpacker.Unpack();
                type = unpacker.Type;
            }

            Assert.NotEmpty(entries);
            Assert.All(entries, entry => Assert.False(string.IsNullOrEmpty(entry.Key)));
            Assert.Equal(ZtrFileType.BigEndianCompressedDictionary, type);
        }

        [Fact]
        public void Pack_Unpack_RoundTrip_PreservesEntries()
        {
            if (!TestPaths.Exists(TestPaths.LooseZtr))
                return;

            ZtrFileEntry[] entries;

            using (FileStream input = File.OpenRead(TestPaths.LooseZtr))
            {
                ZtrFileUnpacker unpacker = new(input, Encoding);
                entries = unpacker.Unpack();
            }

            Assert.NotEmpty(entries);

            string key = entries[0].Key;
            string originalValue = entries[0].Value;
            entries[0].Value = originalValue + "!";

            byte[] packed;

            using (MemoryStream output = new())
            {
                ZtrFilePacker packer = new(output, Encoding, ZtrFileType.BigEndianCompressedDictionary);
                packer.Pack(entries);
                packed = output.ToArray();
            }

            ZtrFileEntry[] result;

            using (MemoryStream input = new(packed))
            {
                ZtrFileUnpacker unpacker = new(input, Encoding);
                result = unpacker.Unpack();
            }

            Assert.Equal(entries.Length, result.Length);
            Assert.Equal(key, result[0].Key);
            Assert.Equal(entries[0].Value, result[0].Value);

            for (int i = 1; i < result.Length; i++)
            {
                Assert.Equal(entries[i].Key, result[i].Key);
                Assert.Equal(entries[i].Value, result[i].Value);
            }
        }

        [Fact]
        public void ReadText_Container_ReturnsZtrPayload()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            byte[] payload;

            using (FileStream input = File.OpenRead(TestPaths.ZtrContainer))
            {
                payload = ZtrContainer.ReadText(input, out ZtrContainerType type);
                Assert.Equal(ZtrContainerType.Wpd, type);
            }

            Assert.NotEmpty(payload);

            ZtrFileEntry[] entries;

            using (MemoryStream input = new(payload))
            {
                ZtrFileUnpacker unpacker = new(input, Encoding);
                entries = unpacker.Unpack();
            }

            Assert.NotEmpty(entries);
            Assert.All(entries, entry => Assert.False(string.IsNullOrEmpty(entry.Key)));
        }

        [Fact]
        public void WriteText_Container_PreservesOtherRecords()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            string temp = TestPaths.CopyToTemp(TestPaths.ZtrContainer);

            try
            {
                byte[] original = File.ReadAllBytes(temp);
                byte[] payload;
                byte[] packed;

                using (MemoryStream source = new(original))
                {
                    payload = ZtrContainer.ReadText(source, out ZtrContainerType _);
                }

                ZtrFileEntry[] entries;

                using (MemoryStream input = new(payload))
                {
                    ZtrFileUnpacker unpacker = new(input, Encoding);
                    entries = unpacker.Unpack();
                }

                Assert.NotEmpty(entries);

                using (MemoryStream output = new())
                {
                    ZtrFilePacker packer = new(output, Encoding, ZtrFileType.BigEndianCompressedDictionary);
                    packer.Pack(entries);
                    packed = output.ToArray();
                }

                byte[] rebuilt;

                using (MemoryStream output = new())
                using (MemoryStream source = new(original))
                {
                    ZtrContainer.WriteText(output, source, packed);
                    rebuilt = output.ToArray();
                }

                using (MemoryStream verify = new(rebuilt))
                {
                    byte[] newPayload = ZtrContainer.ReadText(verify, out ZtrContainerType _);
                    Assert.Equal(packed, newPayload);
                }

                WpdHeader originalHeader;
                WpdHeader rebuiltHeader;

                using (MemoryStream input = new(original))
                    originalHeader = ZtrContainer.ReadHeader(input);

                using (MemoryStream input = new(rebuilt))
                    rebuiltHeader = ZtrContainer.ReadHeader(input);

                Assert.Equal(originalHeader.Count, rebuiltHeader.Count);

                foreach (WpdEntry entry in originalHeader.Entries)
                {
                    if (string.Equals(entry.Extension, "ztr", StringComparison.OrdinalIgnoreCase))
                        continue;

                    WpdEntry match = rebuiltHeader.Entries.FirstOrDefault(e =>
                        string.Equals(e.Extension, entry.Extension, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(e.NameWithoutExtension, entry.NameWithoutExtension, StringComparison.OrdinalIgnoreCase));

                    Assert.NotNull(match);
                    Assert.Equal(entry.Length, match.Length);
                    Assert.Equal(ReadRecord(original, entry), ReadRecord(rebuilt, match));
                }
            }
            finally
            {
                if (File.Exists(temp))
                    File.Delete(temp);
            }
        }

        private static byte[] ReadRecord(byte[] data, WpdEntry entry)
        {
            byte[] result = new byte[entry.Length];
            Buffer.BlockCopy(data, entry.Offset, result, 0, entry.Length);
            return result;
        }
    }
}
