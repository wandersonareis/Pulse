using System;
using System.IO;
using Be.IO;
using Pulse.Core;
using Pulse.FS;
using static System.IO.Directory;

namespace Pulse.Gui.Common
{
    internal static class WpdZtrUnpack
    {
        public static void Extract(string wpdFile)
        {
            MemoryStream ms = new();
            using Stream book = File.OpenRead(wpdFile);
            using BeBinaryReader sr = new(book);

            WdbHeader header = book.ReadContent<WdbHeader>();
            WpdEntry entry = header.Entries[1];

            string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
            CreateDirectory(path);
            string filename = Path.Combine(path, entry.Name);
            sr.BaseStream.Position = header.Entries[1].Offset;
            byte[] arrayByte = sr.ReadBytes(header.Entries[1].Length);
            ms.Write(arrayByte, 0, arrayByte.Length);

            using FileStream file = new(filename, FileMode.OpenOrCreate, FileAccess.Write);
            ms.WriteTo(file);

            //File.WriteAllBytes(filename, ms.ToArray());
        }
    }
}
