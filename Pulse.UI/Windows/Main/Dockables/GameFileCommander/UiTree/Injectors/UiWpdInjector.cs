using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiWpdInjector : IDisposable
    {
        private readonly WpdArchiveListing _listing;
        private readonly WpdEntry[] _leafs;
        private readonly IUiInjectionSource _source;
        private readonly bool? _conversion;
        private readonly Dictionary<string, IWpdEntryInjector> _injectors;
        private readonly Lazy<Stream> _headers;
        private readonly Lazy<Stream> _content;
        private readonly byte[] _buff = new byte[32 * 1024];
        private bool _injected;

        public UiWpdInjector(WpdArchiveListing listing, WpdEntry[] leafs, bool? conversion, IUiInjectionSource source)
        {
            _listing = listing;
            _leafs = leafs;
            _source = source;
            _conversion = conversion;
            _injectors = ProvideInjectors();
            _headers = new(AcquireHeaders);
            _content = new(AcquireContent);
        }

        public void Dispose()
        {
            _headers.NullSafeDispose();
            _content.NullSafeDispose();
        }

        public void Inject(UiInjectionManager manager)
        {
            if (_leafs.Length == 0)
                return;

            string root = _source.ProvideRootDirectory();
            string targetDirectory = Path.Combine(root, _listing.ExtractionSubpath);
            if (!_source.DirectoryIsExists(targetDirectory))
                return;

            foreach (WpdEntry entry  in _leafs)
            {
                string targetPath = Path.Combine(targetDirectory, entry.NameWithoutExtension);
                Inject(entry, targetPath);
            }

            if (!_injected) return;
            {
                List<ArchiveEntry> entries = new(2);
                MemoryInjectionSource memorySource = new();
                if (_headers.IsValueCreated)
                {
                    ArchiveEntry entry = _listing.Accessor.HeadersEntry;
                    entries.Add(entry);
                    memorySource.RegisterStream(entry.Name, _headers.Value);
                }
                if (_content.IsValueCreated)
                {
                    ArchiveEntry entry = _listing.Accessor.ContentEntry;
                    entries.Add(entry);
                    memorySource.RegisterStream(entry.Name, _content.Value);
                }

                using (UiArchiveInjector injector = new(_listing.Accessor.Parent, entries.ToArray(), _conversion, false, memorySource))
                    injector.Inject(manager);

                manager.Enqueue(_listing.Accessor.Parent);
            }
        }

        private void Inject(WpdEntry entry, string targetPath)
        {
            string targetExtension = entry.Extension.ToLowerInvariant();

            if (_injectors.TryGetValue(targetExtension, out IWpdEntryInjector injector))
            {
                string targetFullPath = targetPath + '.' + injector.SourceExtension;
                using (Stream input = _source.TryOpen(targetFullPath))
                {
                    if (input != null)
                    {
                        injector.Inject(entry, input, _headers, _content, _buff);
                        _injected = true;
                        return;
                    }
                }
            }

            if (_conversion != true)
            {
                string targetFullPath = targetPath + '.' + targetExtension;
                using (Stream input = _source.TryOpen(targetFullPath))
                {
                    if (input == null) return;
                    DefaultInjector.Inject(entry, input, _headers, _content, _buff);
                    _injected = true;
                }
            }
        }

        private Dictionary<string, IWpdEntryInjector> ProvideInjectors()
        {
            return _conversion != false ? Converters : Emptry;
        }

        private Stream AcquireHeaders()
        {
            int uncompressedSize = (int)_listing.Accessor.HeadersEntry.UncompressedSize;

            MemoryStream result = new((int)(uncompressedSize * 1.3));
            using (Stream input = _listing.Accessor.ExtractHeaders())
                input.CopyToStream(result, uncompressedSize, _buff);

            return result;
        }

        private Stream AcquireContent()
        {
            int uncompressedSize = (int)_listing.Accessor.ContentEntry.UncompressedSize;

            MemoryStream result = new((int)(uncompressedSize * 1.3));
            using (Stream input = _listing.Accessor.ExtractContent())
                input.CopyToStream(result, uncompressedSize, _buff);

            return result;
        }

        #region Static

        private static readonly IWpdEntryInjector DefaultInjector = ProvideDefaultInjector();
        private static readonly Dictionary<string, IWpdEntryInjector> Emptry = new(0);
        private static readonly Dictionary<string, IWpdEntryInjector> Converters = RegisterConverters();

        private static IWpdEntryInjector ProvideDefaultInjector()
        {
            return new DefaultWpdEntryInjector();
        }

        private static Dictionary<string, IWpdEntryInjector> RegisterConverters()
        {
            return new()
            {
                {"txbh", new DdsToTxbhWpdEntryInjector()},
                {"vtex", new DdsToVtexWpdEntryInjector()}
            };
        }

        #endregion

        public static void InjectSingle(WpdArchiveListing listing, WpdEntry entry, MemoryStream output)
        {
            using (MemoryInjectionSource source = new())
            {
                source.RegisterStream(string.Empty, output);
                UiWpdInjector injector = new(listing, new[] {entry}, false, source);

                UiInjectionManager manager = new();
                injector.Inject(manager);
                manager.WriteListings();
            }
        }
    }
}