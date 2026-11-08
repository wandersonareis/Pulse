using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Xunit;

namespace Pulse.Tests
{
    public sealed class ZtrInnerTests
    {
        private static FFXIIITextEncoding Encoding => FFXIIITextEncodingFactory.DefaultEuroEncoding.Value;

        [Fact]
        public void ResolveInnerZtrName_Container_ReturnsZtrRecordName()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            string name = ZtrTextUnitScanner.ResolveInnerZtrName(File.ReadAllBytes(TestPaths.ZtrContainer), TestPaths.ZtrContainer);

            Assert.False(string.IsNullOrEmpty(name));
            Assert.EndsWith(".ztr", name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TextUnitFile_Container_HasInnerZtr()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            byte[] container = File.ReadAllBytes(TestPaths.ZtrContainer);
            string innerName = ZtrTextUnitScanner.ResolveInnerZtrName(container, TestPaths.ZtrContainer);

            ZtrTextUnitFile file = ZtrTextUnitFile.ProvideContainer(TestPaths.ZtrContainer, innerName);

            Assert.True(file.HasInnerZtr);
            Assert.True(file.HasOuterZtr);
            Assert.False(file.IsLoose);
            Assert.Equal(innerName, file.ProvideInnerName());
            Assert.EndsWith(".ztr", file.ProvideInnerName(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TextUnitFile_Loose_InnerIsTheFileItself()
        {
            if (!TestPaths.Exists(TestPaths.LooseZtr))
                return;

            ZtrTextUnitFile file = ZtrTextUnitFile.ProvideLoose(TestPaths.LooseZtr);

            Assert.True(file.HasInnerZtr);
            Assert.False(file.HasOuterZtr);
            Assert.True(file.IsLoose);
            Assert.Equal(TestPaths.LooseZtr, file.InnerPath);
            Assert.Equal(Path.GetFileName(TestPaths.LooseZtr), file.ProvideInnerName());
        }

        [Fact]
        public void TextUnitFile_ContainerWithoutZtr_InnerIsNull()
        {
            string path = WriteContainerWithoutZtr();

            try
            {
                string inner = ZtrTextUnitScanner.ResolveInnerZtrName(File.ReadAllBytes(path), path);
                Assert.Null(inner);

                ZtrTextUnitFile file = ZtrTextUnitFile.ProvideContainer(path, inner);

                Assert.False(file.HasInnerZtr);
                Assert.False(file.HasOuterZtr);
                Assert.Equal(path, file.Path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void ZtrRecord_PayloadMatchesReadText()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            byte[] original = File.ReadAllBytes(TestPaths.ZtrContainer);

            byte[] payload;
            byte[] record;
            ZtrContainerType containerType;

            using (MemoryStream source = new(original))
                payload = ZtrContainer.ReadText(source, out containerType);

            Assert.Equal(ZtrContainerType.Wpd, containerType);

            using (MemoryStream source = new(original))
            {
                WpdHeader header = ZtrContainer.ReadHeader(source);
                WpdEntry entry = ZtrContainer.FindZtrEntry(header);
                Assert.NotNull(entry);

                source.Position = entry.Offset;
                record = new byte[entry.Length];
                Assert.Equal(entry.Length, source.Read(record, 0, record.Length));
            }

            Assert.Equal(payload, record);
        }

        [Fact]
        public void ZtrRecord_RoundTrip_PreservesOtherRecords()
        {
            if (!TestPaths.Exists(TestPaths.LooseZtr))
                return;

            byte[] original = File.ReadAllBytes(TestPaths.LooseZtr);

            ZtrFileEntry[] entries;
            ZtrFileType type;

            using (MemoryStream input = new(original))
            {
                ZtrFileUnpacker unpacker = new(input, Encoding);
                entries = unpacker.Unpack();
                type = unpacker.Type;
            }

            Assert.NotEmpty(entries);

            entries[0].Value = entries[0].Value + "!";

            byte[] packed;

            using (MemoryStream output = new())
            {
                ZtrFilePacker packer = new(output, Encoding, type);
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

            if (rebuilt.Length == packed.Length)
            {
                Assert.Equal(packed, rebuilt);
                return;
            }

            using (MemoryStream verify = new(rebuilt))
            {
                byte[] newPayload = ZtrContainer.ReadText(verify, out ZtrContainerType _);
                Assert.Equal(packed, newPayload);
            }
        }

        private static int Align(int value)
        {
            return (value + ZtrContainer.DataAlignment - 1) / ZtrContainer.DataAlignment * ZtrContainer.DataAlignment;
        }

        [Fact]
        public void Container_WriteText_WithSamePayload_PreservesRecords()
        {
            if (!TestPaths.Exists(TestPaths.ZtrContainer))
                return;

            byte[] original = File.ReadAllBytes(TestPaths.ZtrContainer);

            WpdHeader originalHeader;
            byte[] payload;

            using (MemoryStream source = new(original))
            {
                originalHeader = ZtrContainer.ReadHeader(source);
                payload = ZtrContainer.ReadText(source, out ZtrContainerType _);
            }

            Assert.True(originalHeader.Count > 1);

            byte[] rebuilt;

            using (MemoryStream output = new())
            using (MemoryStream source = new(original))
            {
                ZtrContainer.WriteText(output, source, payload);
                rebuilt = output.ToArray();
            }

            WpdHeader rebuiltHeader;

            using (MemoryStream input = new(rebuilt))
                rebuiltHeader = ZtrContainer.ReadHeader(input);

            Assert.Equal(originalHeader.Count, rebuiltHeader.Count);

            foreach (WpdEntry entry in originalHeader.Entries)
            {
                if (string.Equals(entry.Extension, "ztr", StringComparison.OrdinalIgnoreCase))
                    continue;

                WpdEntry match = Array.Find(rebuiltHeader.Entries, e =>
                    string.Equals(e.Extension, entry.Extension, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.NameWithoutExtension, entry.NameWithoutExtension, StringComparison.OrdinalIgnoreCase));

                Assert.NotNull(match);
                Assert.Equal(ReadRecord(original, entry), ReadRecord(rebuilt, match));
            }

            using (MemoryStream verify = new(rebuilt))
            {
                byte[] newPayload = ZtrContainer.ReadText(verify, out ZtrContainerType _);
                Assert.Equal(payload, newPayload);
            }
        }

        private static byte[] ReadRecord(byte[] data, WpdEntry entry)
        {
            byte[] result = new byte[entry.Length];
            Buffer.BlockCopy(data, entry.Offset, result, 0, entry.Length);
            return result;
        }

        private static string WriteContainerWithoutZtr()
        {
            string path = Path.Combine(Path.GetTempPath(), "pulse-tests", Guid.NewGuid().ToString("N") + "-noztr.bin");

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            const int count = 2;

            byte[] result = new byte[ZtrContainer.HeaderSize + count * ZtrContainer.RecordSize + ZtrContainer.DataAlignment * count];

            Buffer.BlockCopy(new byte[] {0x57, 0x50, 0x44, 0x00}, 0, result, 0, 4);
            result[4] = 0;
            result[5] = 0;
            result[6] = 0;
            result[7] = (byte)count;

            for (int i = 0; i < count; i++)
            {
                int offset = ZtrContainer.HeaderSize + i * ZtrContainer.RecordSize;
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("dummy"), 0, result, offset, 5);
                Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("wdb"), 0, result, offset + 24, 3);

                int dataOffset = Align(ZtrContainer.HeaderSize + count * ZtrContainer.RecordSize + i * ZtrContainer.DataAlignment);
                result[offset + 19] = (byte)(dataOffset >> 24);
                result[offset + 20] = (byte)(dataOffset >> 16);
                result[offset + 21] = (byte)(dataOffset >> 8);
                result[offset + 22] = (byte)dataOffset;

                int length = 16;
                result[offset + 23] = (byte)(length >> 24);
                result[offset + 24] = (byte)(length >> 16);
                result[offset + 25] = (byte)(length >> 8);
                result[offset + 26] = (byte)length;
            }

            File.WriteAllBytes(path, result);
            return path;
        }
    }
}
