using System;
using System.IO;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using Yusnaan.Common;

namespace Yusnaan.Model.Injectors
{
    internal class WpdEntryInjector
    {
        public void Pack(string? wpdFile)
        {
            using MemoryStream ms = new();
            string? wpdFileName = Path.GetFileName(wpdFile);
            FileInfo ztrFile;

            using (Stream wpdFileStream = File.OpenRead(wpdFile ?? throw new ArgumentNullException(nameof(wpdFile))))
            {
                var header = wpdFileStream.ReadContent<WdbHeader>();
                WpdEntry entry = header.Entries[1];

                if (!string.Equals(entry.Extension, "ztr", StringComparison.Ordinal))
                {
                    throw new ScenarieIncompatibleException("This is a file with scenario texts?");
                }

                string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
                string ztrFilePath = Path.Combine(path, entry.Name);
                ztrFile = new((File.Exists(ztrFilePath) ? ztrFilePath : Dialogs.GetNewZtrFile(ztrFilePath)) ?? throw new InvalidOperationException());

                if (!string.Equals(ztrFile.Name, entry.Name, StringComparison.Ordinal) && !ztrFile.Exists)
                {
                    new TaskDialogs().ShowWarningDialog("", "", $@"File {ztrFile.Name} not exist or is not correct ztr file.");
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
            Directory.CreateDirectory(newPath);

            using FileStream file = new($"{newPath}\\{wpdFileName}", FileMode.OpenOrCreate, FileAccess.Write);
            ms.WriteTo(file);

            //File.WriteAllBytes($"{newPath}\\{wpdFileName}", ms.ToArray());
        }

        public void TestPack(FileInfo? wpdFile)
        {
            ArgumentNullException.ThrowIfNull(wpdFile);

            using MemoryStream ms = new();
            string wpdFileName = wpdFile.Name;
            Logger.Log<WpdEntryInjector>(Logger.Level.Info, $"Opening file: {wpdFileName}");

            FileInfo ztrFile;

            using (Stream wpdFileStream = wpdFile.OpenRead())
            {
                var header = wpdFileStream.ReadContent<WdbHeader>();
                WpdEntry entry = header.Entries[1];

                if (!string.Equals(entry.Extension, "ztr", StringComparison.Ordinal))
                {
                    throw new ScenarieIncompatibleException("This is a file with scenario texts?");
                }

                string ztrFilePath = Path.Combine(wpdFile.DirectoryName ?? throw new InvalidOperationException(), entry.NameWithoutExtension, entry.Name);
                ztrFile = new FileInfo((File.Exists(ztrFilePath) ? ztrFilePath : Dialogs.GetNewZtrFile(ztrFilePath)) ?? throw new InvalidOperationException());

                if (!string.Equals(ztrFile.Name, entry.Name, StringComparison.Ordinal) && !ztrFile.Exists)
                {
                    new TaskDialogs().ShowWarningDialog("", "", $@"File {ztrFile.Name} not exist or is not correct ztr file.");
                    return;
                }

                using FileStream fileRead = ztrFile.OpenRead();
                Logger.Log<WpdEntryInjector>(Logger.Level.Info, $"Opening file: {ztrFile.Name}");
                Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr file position: {entry.Offset:X}"));
                Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr old size: {entry.Length:X}"));

                entry.Length = (int)fileRead.Length;
                header.WriteToStream(ms);
                wpdFileStream.CopyTo(ms);
                ms.Position = entry.Offset;

                fileRead.CopyTo(ms);
                
                Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr file new size: {entry.Length:X}"));
            }

            string newPath = Path.Combine(ztrFile.DirectoryName ?? throw new InvalidOperationException(), "_reImported");
            Directory.CreateDirectory(newPath);

            using FileStream file = new($"{newPath}\\{wpdFileName}", FileMode.Create, FileAccess.Write);
            ms.WriteTo(file);
        }
    }
}
