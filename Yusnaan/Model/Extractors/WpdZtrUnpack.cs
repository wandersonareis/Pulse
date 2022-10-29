using System;
using System.IO;
using System.Threading.Tasks;
using Be.IO;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using FileEx = Yusnaan.Common.FileEx;

namespace Yusnaan.Model.Extractors;

internal sealed class WpdZtrUnpack
{
    /*public void Extract(string wpdFile, out FullPath? fileName)
    {
        using Stream wpdFileStream = File.OpenRead(wpdFile);
        using BeBinaryReader sr = new(wpdFileStream);

        MemoryStream ms = new();

        var header = wpdFileStream.ReadContent<WdbHeader>();
        WpdEntry entry = header.Entries[1];

        if (!string.Equals(entry.Extension, "ztr", StringComparison.OrdinalIgnoreCase))
        {
            fileName = null;

            new TaskDialogs().ShowSkipDialog("It's was skipped", "This is a file with scenario texts?", $"This {Path.GetFileName(wpdFile)} contains Dummy ztr!");
            return;
        }

        if (string.Equals(entry.NameWithoutExtension, "dummy", StringComparison.Ordinal))
        {
            fileName = null;

            new TaskDialogs().ShowSkipDialog("File empty", "It's was skipped.", $"This {Path.GetFileName(wpdFile)} contains Dummy ztr!");
            return;
        }
        string path = Path.Combine(Path.GetDirectoryName(wpdFile) ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
        Directory.CreateDirectory(path);
        //string? newPath = Path.Combine(path, entry.Name);
        //fileName = File.Exists(newPath) ? newPath : Dialogs.GetNewZtrFile(newPath);
        fileName = FullPath.FromPath(Path.Combine(path, entry.Name));
        sr.BaseStream.Position = header.Entries[1].Offset;
        byte[] arrayByte = sr.ReadBytes(header.Entries[1].Length);
        ms.Write(arrayByte, 0, arrayByte.Length);

        using FileStream fileStream = new(fileName.Value.Value ?? throw new ArgumentNullException(nameof(fileName)), FileMode.Create, FileAccess.ReadWrite);
        ms.WriteTo(fileStream);
    }*/
    public async ValueTask<FullPath> ZtrEntryExtractAsync(FileInfo wpdFile)
    {
        ArgumentNullException.ThrowIfNull(wpdFile);
        
        await using Stream wpdFileStream = wpdFile.OpenRead();
        Logger.Log<WpdZtrUnpack>(Logger.Level.Info, $"Opening file: {wpdFile.Name}");
        using BeBinaryReader beBinaryReader = new(wpdFileStream);

        MemoryStream ms = new();

        var header = wpdFileStream.ReadContent<WdbHeader>();
        WpdEntry entry = header.Entries[1];

        if (!string.Equals(entry.Extension, "ztr", StringComparison.OrdinalIgnoreCase))
        {
            new TaskDialogs().ShowSkipDialog("It's was skipped", $"This {wpdFile.Name} is a file with scenario texts?", $"");
            Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"This {wpdFile.Name} is a file with scenario texts?");
            return FullPath.Empty;
        }

        Logger.Log<WpdZtrUnpack>(Logger.Level.Info, $"Source ztr file: {entry.Name}");
        Logger.Log<WpdZtrUnpack>(Logger.Level.Info, FormattableString.Invariant($"Ztr file position: {entry.Offset:X}"));
        Logger.Log<WpdZtrUnpack>(Logger.Level.Info, FormattableString.Invariant($"Ztr file size: {entry.Length:X}"));

        string path = Path.Combine(wpdFile.DirectoryName ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
        Directory.CreateDirectory(path);
        FullPath ztrFileName = FullPath.FromPath(Path.Combine(path, entry.Name));
        beBinaryReader.BaseStream.Position = header.Entries[1].Offset;
        //byte[] arrayByte = beBinaryReader.ReadBytes(header.Entries[1].Length);
        ReadOnlyMemory<byte> arrayBytes = beBinaryReader.ReadBytes(header.Entries[1].Length);
        await ms.WriteAsync(arrayBytes);

        await using FileStream outputFileStream = new(ztrFileName.Value ?? throw new ArgumentNullException(nameof(ztrFileName)), FileEx.FileStreamOutputOptions());
        ms.WriteTo(outputFileStream);
        
        Logger.Log<WpdZtrUnpack>(Logger.Level.Info, $"Extracted file: {ztrFileName.Name}");
        return ztrFileName;
    }
}