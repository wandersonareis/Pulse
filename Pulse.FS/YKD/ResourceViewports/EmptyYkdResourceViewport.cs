using System.IO;

namespace Pulse.FS
{
    public sealed class EmptyYkdResourceViewport : YkdResourceViewport
    {
        public override YkdResourceViewportType Type => YkdResourceViewportType.Empty;

        public override int Size => 0;

        public override void ReadFromStream(Stream stream)
        {
        }

        public override void WriteToStream(Stream stream)
        {
        }

        public override YkdResourceViewport Clone()
        {
            return new EmptyYkdResourceViewport();
        }
    }
}