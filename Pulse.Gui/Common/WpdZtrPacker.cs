using System;
using static System.IO.Directory;
using System.IO;
using System.Windows.Forms;
using Be.IO;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.Gui.Common
{
    internal static class WpdZtrPacker
    {
        public static void Pack(string wpdFile)
        {
            using MemoryStream ms = new();
            string wpdFileName = Path.GetFileName(wpdFile);
            FileInfo ztrFile;

            using (Stream book = File.OpenRead(wpdFile))
            {
                WdbHeader header = book.ReadContent<WdbHeader>();
                WpdEntry entry = header.Entries[1];
                string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
                ztrFile = new FileInfo(Path.Combine(path, entry.Name));

                if (ztrFile.Name != entry.Name)
                {
                    MessageBox.Show($@"File {ztrFile.Name} is not right ztr file.");
                    return;
                }
                if (!ztrFile.Exists)
                {
                    MessageBox.Show($@"File {ztrFile.Name} not founded.");
                    return;
                }

                using FileStream fileRead = File.OpenRead(ztrFile.FullName);
                entry.Length = (int)fileRead.Length;
                header.WriteToStream(ms);
                book.CopyTo(ms);
                ms.Position = entry.Offset;

                fileRead.CopyTo(ms);
            }

            string newPath = Path.Combine(ztrFile.DirectoryName ?? throw new InvalidOperationException(), "_reImported");
            CreateDirectory(newPath);

            using FileStream file = new($"{newPath}\\{wpdFileName}", FileMode.OpenOrCreate, FileAccess.Write);
            ms.WriteTo(file);

            //File.WriteAllBytes($"{newPath}\\{wpdFileName}", ms.ToArray());
        }
    }
}
