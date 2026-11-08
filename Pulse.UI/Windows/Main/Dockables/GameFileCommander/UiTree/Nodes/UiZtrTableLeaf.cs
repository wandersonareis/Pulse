using System.Windows.Media;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiZtrTableLeaf : UiNode, IUiLeaf
    {
        public WpdEntry Entry { get; }
        public WpdArchiveListing Listing { get; }
        public ArchiveListing ContainerListing => Listing.Accessor.Parent;
        public ArchiveEntry ContainerEntry => Listing.Accessor.HeadersEntry;

        public UiZtrTableLeaf(string name, WpdEntry entry, WpdArchiveListing listing)
            : base(name, UiNodeType.ZtrRecordLeaf)
        {
            Entry = entry;
            Listing = listing;
        }

        public override ImageSource Icon => Icons.TxtFileIcon;
    }
}
