using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWdbMovieLeaf : UiNode, IUiLeaf
    {
        public WdbMovieEntry Entry { get; }
        public WdbMovieArchiveListing Listing { get; }

        public UiWdbMovieLeaf(string name, WdbMovieEntry entry, WdbMovieArchiveListing listing)
            : base(name, UiNodeType.DataTableLeaf)
        {
            Entry = entry;
            Listing = listing;
        }
    }
}