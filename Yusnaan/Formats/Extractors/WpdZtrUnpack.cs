using System;
using System.IO;
using Be.IO;
using Pulse.Core;
using Pulse.FS;
using Yusnaan.Common;
using static System.IO.Directory;

namespace Yusnaan.Formats
{
    internal static class WpdZtrUnpack
    {
        public static void Extract(string wpdFile) => Extract(wpdFile, out string? fileName);

        public static void Extract(string wpdFile, out string? fileName)
        {
            using Stream book = File.OpenRead(wpdFile);
            using BeBinaryReader sr = new(book);

            MemoryStream ms = new();

            WdbHeader header = book.ReadContent<WdbHeader>();
            WpdEntry entry = header.Entries[1];

            if (entry.Extension != "ztr")
            {
                _ = System.Windows.MessageBox.Show(ResourceLr.StrFile);
                fileName = null;
                return;
            }

            string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
            CreateDirectory(path);
            //string? newPath = Path.Combine(path, entry.Name);
            //fileName = File.Exists(newPath) ? newPath : Dialogs.GetNewZtrFile(newPath);
            fileName = Path.Combine(path, entry.Name);
            sr.BaseStream.Position = header.Entries[1].Offset;
            byte[] arrayByte = sr.ReadBytes(header.Entries[1].Length);
            ms.Write(arrayByte, 0, arrayByte.Length);

            using FileStream fileStream = new(fileName ?? throw new ArgumentNullException(nameof(fileName)), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ms.WriteTo(fileStream);
        }
    }
}
