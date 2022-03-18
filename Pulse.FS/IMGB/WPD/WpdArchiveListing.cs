using System.Collections.Generic;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WpdArchiveListing : List<WpdEntry>, IArchiveListing
    {
        public readonly ImgbArchiveAccessor Accessor;

        public WpdArchiveListing(ImgbArchiveAccessor accessor)
        {
            Accessor = accessor;
        }

        public WpdArchiveListing(ImgbArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public string Name => Accessor.Name;

        public string ExtractionSubpath => PathEx.ChangeMultiDotExtension(Name, ".unpack");
    }
}