using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using Yusnaan.Common;
using static System.IO.Directory;

namespace Yusnaan.Formats
{
    internal static class ZtrToWpdEntryInjector
    {
        public static void Pack(string? wpdFile)
        {
            using MemoryStream ms = new();
            string? wpdFileName = Path.GetFileName(wpdFile);
            FileInfo ztrFile;

            using (Stream wpdFileStream = File.OpenRead(wpdFile ?? throw new ArgumentNullException(nameof(wpdFile))))
            {
                WdbHeader header = wpdFileStream.ReadContent<WdbHeader>();
                WpdEntry entry = header.Entries[1];

                if (entry.Extension != "ztr")
                {
                    _ = System.Windows.MessageBox.Show(ResourceLr.StrFile);
                    return;
                }

                string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
                string ztrFilePath = Path.Combine(path, entry.Name);
                ztrFile = new FileInfo((File.Exists(ztrFilePath) ? ztrFilePath : Dialogs.GetNewZtrFile(ztrFilePath)) ?? throw new InvalidOperationException());

                if (ztrFile.Name != entry.Name && !ztrFile.Exists)
                {
                    _ = System.Windows.MessageBox.Show($@"File {ztrFile.Name} not exist or is not correct ztr file.");
                    return;
                }

                using FileStream fileRead = File.OpenRead(ztrFile.FullName);
                entry.Length = (int)fileRead.Length;
                header.WriteToStream(ms);
                wpdFileStream.CopyTo(ms);
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
