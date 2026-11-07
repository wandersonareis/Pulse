using System;
using System.Windows.Media;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiTextUnitLeaf : UiNode, IUiLeaf
    {
        public ArchiveEntry Entry { get; }
        public ArchiveListing Listing { get; }
        public string Language { get; }

        public bool IsEditable => string.Equals(Language, "us", StringComparison.OrdinalIgnoreCase);

        public UiTextUnitLeaf(string language, ArchiveEntry entry, ArchiveListing listing)
            : base(language, UiNodeType.TextUnitLeaf)
        {
            Language = language;
            Entry = entry;
            Listing = listing;
        }

        public override ImageSource Icon
        {
            get
            {
                return IsEditable ? Icons.TxtFileIcon : base.Icon;
            }
        }
    }
}
