using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WdbMovieEntry : IArchiveEntry, IStreamingContent
    {
        public const int StructSize = 16;

        public int PackageNameOffset;
        public int Length;
        public int Dummy;
        public uint Offset;

        public WpdEntry Entry { get; set; }
        public string PackageName  { get; set; }
        public string Name => $"[{PackageName}] {Entry.Name}";

        public unsafe void ReadFromStream(Stream stream)
        {
            byte[] data = stream.EnsureRead(StructSize);
            fixed (byte* b = data)
            {
                PackageNameOffset = Endian.ToBigInt32(b + 0 * 4);
                Length = Endian.ToBigInt32(b + 1 * 4);
                Dummy = Endian.ToBigInt32(b + 2 * 4);
                Offset = Endian.ToBigUInt32(b + 3 * 4);
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.WriteBig(PackageNameOffset);
            bw.WriteBig(Length);
            bw.WriteBig(Dummy);
            bw.WriteBig(Offset);
        }
    }
}