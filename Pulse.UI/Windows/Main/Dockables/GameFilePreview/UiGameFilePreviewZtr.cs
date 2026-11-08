using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;
using Pulse.FS;

// ReSharper disable UnusedMember.Local

namespace Pulse.UI
{
    public sealed class UiGameFilePreviewZtr : UiGrid
    {
        private readonly UiTextBox _textBox;

        public UiGameFilePreviewZtr()
        {
            #region Constructor

            _textBox = new() {TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, IsReadOnly = true, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto};
            AddUiElement(_textBox, 0, 0);

            #endregion
        }

        private ArchiveListing _listing;
        private ArchiveEntry _entry;

        public void Show(ArchiveListing listing, ArchiveEntry entry)
        {
            _listing = listing;
            _entry = entry;

            if (listing == null || entry == null)
            {
                _textBox.Text = null;
                return;
            }

            FFXIIITextEncoding encoding = InteractionService.TextEncoding.Provide().Encoding;

            ZtrFileEntry[] entries;
            using (Stream data = listing.Accessor.ExtractBinary(entry))
            using (MemoryStream buffer = new())
            {
                data.CopyTo(buffer);
                buffer.Position = 0;

                byte[] text = ZtrContainer.ReadText(buffer, out ZtrContainerType _);

                entries = Unpack(text, encoding);
            }

            Show(entry.Name, entries);
        }

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            _listing = null;
            _entry = null;

            if (listing == null || entry == null)
            {
                _textBox.Text = null;
                return;
            }

            FFXIIITextEncoding encoding = InteractionService.TextEncoding.Provide().Encoding;

            ZtrFileEntry[] entries;
            using (Stream data = listing.Accessor.ExtractHeaders())
            using (MemoryStream buffer = new())
            {
                data.CopyTo(buffer);
                buffer.Position = entry.Offset;

                using (MemoryStream payload = new(entry.Length))
                {
                    buffer.CopyTo(payload, entry.Length);
                    payload.Position = 0;

                    entries = Unpack(payload.ToArray(), encoding);
                }
            }

            Show(entry.Name, entries);
        }

        private static ZtrFileEntry[] Unpack(byte[] payload, FFXIIITextEncoding encoding)
        {
            if (payload == null || payload.Length == 0)
                return Array.Empty<ZtrFileEntry>();

            using (MemoryStream input = new(payload))
            {
                ZtrFileUnpacker unpacker = new(input, encoding);
                return unpacker.Unpack();
            }
        }

        private void Show(string name, ZtrFileEntry[] entries)
        {
            _listing = null;
            _entry = null;

            if (entries.IsNullOrEmpty())
                return;

            using (MemoryStream ms = new(4096))
            {
                ZtrTextWriter writer = new(ms, StringsZtrFormatter.Instance);
                writer.Write(name, entries);

                ms.Position = 0;
                using (StreamReader sr = new(ms, System.Text.Encoding.UTF8, false))
                    _textBox.Text = sr.ReadToEnd();
            }

            Visibility = Visibility.Visible;
        }
    }
}