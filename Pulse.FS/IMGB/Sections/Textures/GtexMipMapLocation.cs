using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class GtexMipMapLocation : IStreamingContent
    {
        public int Offset;
        public int Length;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new(stream);
            Offset = br.ReadBigInt32();
            Length = br.ReadBigInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new(stream);
            bw.WriteBig(Offset);
            bw.WriteBig(Length);
        }
    }
}