using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiTextUnitLeafsAccessor : IUiLeafsAccessor
    {
        private readonly ArchiveListing _listing;
        private readonly UiTextUnitLeaf[] _leafs;
        private readonly bool? _compression;

        public UiNodeType Type => UiNodeType.TextUnit;

        public UiTextUnitLeafsAccessor(ArchiveListing listing, bool? compression, params UiTextUnitLeaf[] leafs)
        {
            _listing = listing;
            _leafs = leafs;
            _compression = compression;
        }

        public void Extract(IUiExtractionTarget target)
        {
            UiTextUnitExtractor extractor = new(_listing, _leafs, target);
            extractor.Extract();
        }

        public void Inject(IUiInjectionSource source, UiInjectionManager manager)
        {
            UiTextUnitInjector injector = new(_listing, _leafs, _compression, source);
            injector.Inject(manager);
        }
    }
}
