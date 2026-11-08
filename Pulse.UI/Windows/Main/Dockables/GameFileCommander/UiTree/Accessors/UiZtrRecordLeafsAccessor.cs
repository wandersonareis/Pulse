using System;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiZtrRecordLeafsAccessor : IUiLeafsAccessor
    {
        private readonly WpdArchiveListing _listing;
        private readonly UiZtrTableLeaf[] _leafs;
        private readonly bool? _compression;

        public UiNodeType Type => UiNodeType.ZtrRecordLeaf;

        public UiZtrRecordLeafsAccessor(WpdArchiveListing listing, bool? compression, params UiZtrTableLeaf[] leafs)
        {
            _listing = listing;
            _leafs = leafs;
            _compression = compression;
        }

        public void Extract(IUiExtractionTarget target)
        {
            UiZtrRecordExtractor extractor = new(_listing, _leafs, target);
            extractor.Extract();
        }

        public void Inject(IUiInjectionSource source, UiInjectionManager manager)
        {
            UiZtrRecordInjector injector = new(_listing, _leafs, _compression, source);
            injector.Inject(manager);
        }
    }
}
