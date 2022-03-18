using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdTableLeaf : UiNode, IUiLeaf
    {
        public WpdEntry Entry { get; }
        public WpdArchiveListing Listing { get; }

        public UiWpdTableLeaf(string name, WpdEntry entry, WpdArchiveListing listing)
            : base(name, UiNodeType.FileTableLeaf)
        {
            Entry = entry;
            Listing = listing;
        }
    }
}