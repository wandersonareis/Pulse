using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveInjector : IDisposable
    {
        private readonly ArchiveListing _listing;
        private readonly ArchiveEntry[] _leafs;
        private readonly IUiInjectionSource _source;
        private readonly Dictionary<string, IArchiveEntryInjector> _injectors;
        private readonly byte[] _buff = new byte[32 * 1024];
        private readonly bool? _conversion;
        private readonly bool? _compression;
        private readonly ArchiveEntryInjectionData _injectionData;
        private bool _injected;

        public UiArchiveInjector(ArchiveListing listing, ArchiveEntry[] leafs, bool? conversion, bool? compression, IUiInjectionSource source)
        {
            _listing = listing;
            _leafs = leafs;
            _source = source;
            _conversion = conversion;
            _compression = compression;
            _injectionData = new ArchiveEntryInjectionData(_listing, OpenOutputStream);
            _injectors = ProvideInjectors();
        }

        public void Dispose()
        {
        }

        public void Inject(UiInjectionManager manager)
        {
            if (_leafs.Length == 0)
                return;

            string root = _source.ProvideRootDirectory();

            foreach (ArchiveEntry entry in _leafs)
            {
                string sourcePath = Path.Combine(root, PathEx.ChangeMultiDotExtension(entry.Name, null));
                string directoryPath = Path.GetDirectoryName(sourcePath);

                if (entry.Name.EndsWith(".ztr"))
                {
                    if (_source.TryProvideStrings() == null && !_source.DirectoryIsExists(directoryPath))
                        continue;
                }
                else if (!_source.DirectoryIsExists(directoryPath))
                    continue;

                Inject(entry, sourcePath);
            }

            if (_injected)
                manager.Enqueue(_listing);
        }

        private void Inject(ArchiveEntry entry, string sourcePath)
        {
            string sourceExtension = PathEx.GetMultiDotComparableExtension(entry.Name);
            string sourceFullPath = sourcePath + sourceExtension;

            IArchiveEntryInjector injector;
            if (_injectors.TryGetValue(sourceExtension, out injector))
            {
                sourceFullPath = sourcePath + injector.SourceExtension;
                if (injector.TryInject(_source, sourceFullPath, _injectionData, entry))
                {
                    _injected = true;
                    return;
                }
            }

            if (_conversion != true)
            {
                if (DefaultInjector.TryInject(_source, sourceFullPath, _injectionData, entry))
                    _injected = true;
            }
        }

        private Stream OpenOutputStream(ArchiveEntry entry)
        {
            MemoryStream ms = new MemoryStream((int)(entry.UncompressedSize * 1.3));

            DisposableStream writer = new DisposableStream(ms);
            writer.BeforeDispose.Add(new DisposableAction(() => _listing.Accessor.OnWritingCompleted(entry, ms, _compression)));

            return writer;
        }

        private Dictionary<string, IArchiveEntryInjector> ProvideInjectors()
        {
            return _conversion != false ? Converters : Emptry;
        }

        #region Static

        private static readonly IArchiveEntryInjector DefaultInjector = ProvideDefaultInjector();
        private static readonly Dictionary<string, IArchiveEntryInjector> Emptry = new Dictionary<string, IArchiveEntryInjector>(0);
        private static readonly Dictionary<string, IArchiveEntryInjector> Converters = RegisterConverters();

        private static IArchiveEntryInjector ProvideDefaultInjector()
        {
            return new DefaultArchiveEntryInjector();
        }

        private static Dictionary<string, IArchiveEntryInjector> RegisterConverters()
        {
            return new Dictionary<string, IArchiveEntryInjector>
            {
                {".ztr", new StringsToZtrArchiveEntryInjector()}
            };
        }

        #endregion
    }
}