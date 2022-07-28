using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveExtractor : IDisposable
    {
        private readonly ArchiveListing _listing;
        private readonly ArchiveEntry[] _leafs;
        private readonly IUiExtractionTarget _target;
        private readonly bool? _conversion;
        private readonly Dictionary<string, IArchiveEntryExtractor> _extractors;

        public UiArchiveExtractor(ArchiveListing listing, ArchiveEntry[] leafs, bool? conversion, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _target = target;
            _conversion = conversion;
            _extractors = ProvideExtractors();
        }

        public void Dispose()
        {
        }

        public void Extract()
        {
            if (_leafs.Length == 0)
                return;

            string root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();

            byte[] buff = new byte[32 * 1024];
            foreach (ArchiveEntry entry in _leafs)
            {
                string targetExtension;
                IArchiveEntryExtractor extractor = GetExtractor(entry, out targetExtension);
                if (extractor == null)
                    continue;
                
                string targetPath = Path.Combine(root, PathEx.ChangeMultiDotExtension(entry.Name, targetExtension));
                string directoryPath = Path.GetDirectoryName(targetPath);
                _target.CreateDirectory(directoryPath);

                using (Stream input = _listing.Accessor.ExtractBinary(entry))
                using (StreamSequence output = _target.Create(targetPath))
                    extractor.Extract(entry, output, input, buff);
            }
        }

        private IArchiveEntryExtractor GetExtractor(ArchiveEntry entry, out string targetExtension)
        {
            IArchiveEntryExtractor result;
            targetExtension = PathEx.GetMultiDotComparableExtension(entry.Name);

            if (entry.Name.Contains("_jp") || entry.Name.Contains("_kr"))
                result = DefaultExtractor;
            else if (_extractors.TryGetValue(targetExtension, out result))
                targetExtension = result.TargetExtension;
            else if (_conversion != true)
                result = DefaultExtractor;

            return result;
        }

        private Dictionary<string, IArchiveEntryExtractor> ProvideExtractors()
        {
            return _conversion != false ? Converters : Emptry;
        }

        #region Static

        private static readonly IArchiveEntryExtractor DefaultExtractor = ProvideDefaultExtractor();
        private static readonly Dictionary<string, IArchiveEntryExtractor> Emptry = new(0);
        private static readonly Dictionary<string, IArchiveEntryExtractor> Converters = RegisterConverters();

        private static IArchiveEntryExtractor ProvideDefaultExtractor()
        {
            return new DefaultArchiveEntryExtractor();
        }

        private static Dictionary<string, IArchiveEntryExtractor> RegisterConverters()
        {
            return new()
            {
                {".ztr", new ZtrToStringsArchiveEntryExtractor()},
                {".win32.scd", new ScdToWaveArchiveEntryExtractor()}
            };
        }

        #endregion
    }
}