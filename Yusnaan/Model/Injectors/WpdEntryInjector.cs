using System;
using System.IO;
using System.Threading.Tasks;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using FileEx = Yusnaan.Common.FileEx;

namespace Yusnaan.Model.Injectors;

internal class WpdEntryInjector
{
    public void Pack(string? wpdFile)
    {
        ArgumentNullException.ThrowIfNull(wpdFile);

        string? wpdFileName = Path.GetFileName(wpdFile);
        byte[] container = File.ReadAllBytes(wpdFile);

        using MemoryStream source = new(container);
        WpdHeader header = ZtrContainer.ReadHeader(source);
        WpdEntry entry = ZtrContainer.FindZtrEntry(header) ?? throw new ScenarieIncompatibleException("This is a file with scenario texts?");

        FileInfo? ztrFile = ResolveZtrFile(wpdFile, entry);
        if (ztrFile == null)
            return;

        byte[] ztrBytes = File.ReadAllBytes(ztrFile.FullName);

        using MemoryStream ms = new();
        ZtrContainer.WriteText(ms, source, ztrBytes);

        string newPath = Path.Combine(ztrFile.DirectoryName ?? throw new InvalidOperationException(), "_reImported");
        Directory.CreateDirectory(newPath);

        using FileStream outputFileStream = new($"{newPath}\\{wpdFileName}", FileEx.FileStreamOutputOptions());
        ms.WriteTo(outputFileStream);
    }

    public async ValueTask PackAsync(FileInfo? wpdFile)
    {
        ArgumentNullException.ThrowIfNull(wpdFile);

        string wpdFileName = wpdFile.Name;
        Logger.Log<WpdEntryInjector>(Logger.Level.Info, $"Opening file: {wpdFileName}");

        byte[] container = await File.ReadAllBytesAsync(wpdFile.FullName);

        await using MemoryStream source = new(container);
        WpdHeader header = ZtrContainer.ReadHeader(source);
        WpdEntry entry = ZtrContainer.FindZtrEntry(header) ?? throw new ScenarieIncompatibleException("This is a file with scenario texts?");

        FileInfo? ztrFile = ResolveZtrFile(wpdFile.DirectoryName, entry);
        if (ztrFile == null)
            return;

        Logger.Log<WpdEntryInjector>(Logger.Level.Info, $"Opening file: {ztrFile.Name}");
        Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr file position: {entry.Offset:X}"));
        Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr old size: {entry.Length:X}"));

        byte[] ztrBytes = await File.ReadAllBytesAsync(ztrFile.FullName);

        using MemoryStream ms = new();
        ZtrContainer.WriteText(ms, source, ztrBytes);

        Logger.Log<WpdEntryInjector>(Logger.Level.Info, FormattableString.Invariant($"Ztr file new size: {ztrBytes.Length:X}"));

        if (ztrFile.Directory?.Parent != null)
        {
            string newPath = Path.Combine(ztrFile.Directory.Parent.FullName, "_reImported");
            Directory.CreateDirectory(newPath);

            await using FileStream file = new($"{newPath}\\{wpdFileName}", FileEx.FileStreamOutputOptions());
            ms.WriteTo(file);
        }
    }

    private static FileInfo? ResolveZtrFile(string? directoryName, WpdEntry entry)
    {
        string path = Path.Combine(directoryName ?? throw new InvalidOperationException(), entry.NameWithoutExtension);
        string ztrFilePath = Path.Combine(path, entry.Name);
        FileInfo ztrFile = new((File.Exists(ztrFilePath) ? ztrFilePath : Dialogs.GetNewZtrFile(ztrFilePath)) ?? throw new InvalidOperationException());

        if (!string.Equals(ztrFile.Name, entry.Name, StringComparison.Ordinal) && !ztrFile.Exists)
        {
            new TaskDialogs().ShowWarningDialog("", "", $@"File {ztrFile.Name} not exist or is not correct ztr file.");
            return null;
        }

        return ztrFile;
    }
}
