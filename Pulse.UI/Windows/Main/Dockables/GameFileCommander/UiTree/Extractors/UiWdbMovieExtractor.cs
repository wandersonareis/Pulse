using System;
using System.Collections.Generic;
using System.IO;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWdbMovieExtractor : IDisposable
    {
        private readonly WdbMovieArchiveListing _listing;
        private readonly WdbMovieEntry[] _leafs;
        private readonly bool? _conversion;
        private readonly IUiExtractionTarget _target;
        private readonly Dictionary<string, IWdbMovieEntryExtractor> _extractors;

        public UiWdbMovieExtractor(WdbMovieArchiveListing listing, WdbMovieEntry[] leafs, bool? conversion, IUiExtractionTarget target)
        {
            _listing = listing;
            _leafs = leafs;
            _conversion = conversion;
            _target = target;
            _extractors = ProvideExtractors(conversion);
        }

        public void Dispose()
        {
        }

        public void Extract()
        {
            if (_leafs.Length == 0)
                return;

            string movieDirectory = InteractionService.GameLocation.Provide().MovieDirectory;
            string packagePostfix = _listing.PackagePostfix;

            string root = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();
            string targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            _target.CreateDirectory(targetDirectory);

            byte[] buff = new byte[32 * 1024];
            foreach (WdbMovieEntry entry in _leafs)
            {
                string targetExtension;
                IWdbMovieEntryExtractor extractor = GetExtractor(entry, out targetExtension);
                if (extractor == null)
                    return;

                string packageDirectory = Path.Combine(targetDirectory, entry.PackageName);
                Directory.CreateDirectory(packageDirectory);

                string sourcePath = Path.Combine(movieDirectory, entry.PackageName + packagePostfix + ".win32.wmp");
                string targetPath = Path.Combine(packageDirectory, entry.Entry.NameWithoutExtension + '.' + targetExtension);
                using (Stream input = File.OpenRead(sourcePath))
                using (Stream output = _target.Create(targetPath))
                    extractor.Extract(entry, output, input, buff);
            }
        }

        private IWdbMovieEntryExtractor GetExtractor(WdbMovieEntry entry, out string targetExtension)
        {
            targetExtension = "bk2";

            return DefaultExtractor;
        }

        private Dictionary<string, IWdbMovieEntryExtractor> ProvideExtractors(bool? conversion)
        {
            return Empty;
        }

        #region Static

        private static readonly IWdbMovieEntryExtractor DefaultExtractor = ProvideDefaultExtractor();
        private static readonly Dictionary<string, IWdbMovieEntryExtractor> Empty = new Dictionary<string, IWdbMovieEntryExtractor>(0);

        private static IWdbMovieEntryExtractor ProvideDefaultExtractor()
        {
            return new DefaultWdbMovieEntryExtractor();
        }

        #endregion
    }
}