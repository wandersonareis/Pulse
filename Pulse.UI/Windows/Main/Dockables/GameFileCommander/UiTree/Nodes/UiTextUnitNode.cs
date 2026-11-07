using System.Collections.Generic;
using System.Windows.Media;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiTextUnitNode : UiLazyContainerNode
    {
        private readonly ArchiveListing _listing;
        private readonly ZtrTextUnit _unit;
        private readonly Dictionary<string, ArchiveEntry> _entries;

        public UiTextUnitNode(ArchiveListing listing, ZtrTextUnit unit, Dictionary<string, ArchiveEntry> entries)
            : base(unit.Id, UiNodeType.TextUnit)
        {
            _listing = listing;
            _unit = unit;
            _entries = entries;
        }

        public ZtrTextUnit Unit => _unit;

        protected override ImageSource GetIcon()
        {
            return Icons.TxtFileIcon;
        }

        protected override UiNode[] ExpandChilds()
        {
            List<UiNode> result = new(_unit.Languages.Count);

            foreach (string language in _unit.Languages)
            {
                if (!_unit.Files.TryGetValue(language, out string path))
                    continue;

                if (!_entries.TryGetValue(path, out ArchiveEntry entry))
                    continue;

                result.Add(new UiTextUnitLeaf(language, entry, _listing) {Parent = this});
            }

            return result.ToArray();
        }
    }
}
