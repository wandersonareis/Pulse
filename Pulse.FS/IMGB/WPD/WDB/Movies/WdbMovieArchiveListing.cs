using System.Collections.Generic;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class WdbMovieArchiveListing : List<WdbMovieEntry>, IArchiveListing
    {
        public readonly DbArchiveAccessor Accessor;

        public WdbMovieArchiveListing(DbArchiveAccessor accessor, int entriesCount)
            : base(entriesCount)
        {
            Accessor = accessor;
        }

        public string Name => Accessor.Name;

        public string ExtractionSubpath => PathEx.ChangeMultiDotExtension(Name, ".unpack");

        public string PackagePostfix => PathEx.ChangeMultiDotExtension(Name, null).EndsWith("_us") ? "_us" : string.Empty;
    }
}