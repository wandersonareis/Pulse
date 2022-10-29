using System.Diagnostics;
using System.Linq;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using Yusnaan.Compressor;
using Yusnaan.ViewModels;

namespace Yusnaan.Model.ztr;

internal class StringsFileCompressor
{
    private readonly FileStream _stream;
    private readonly string _ztrFile;

    public StringsFileCompressor(FileStream stringsStream, string ztrFile)
    {
        _stream = stringsStream;
        _ztrFile = ztrFile;
    }
    /// <summary>
    /// Compress strings file using ZtrCompressor
    /// </summary>
    /// <returns></returns>
    public async ValueTask PackStringsNewCompression()
    {
        ZtrFileHandler ztrFileHandler = new();
        ZtrTextReader reader = new(_stream, StringsZtrFormatter.Instance);
        Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value);

        string? ztr = Dialogs.GetZtrFile(name, _ztrFile);
        if (ztr == null) return;
            
        Logger.Log<StringsFileCompressor>(Logger.Level.Info, $"Opening file: {_stream.Name}");
        Logger.Log<StringsFileCompressor>(Logger.Level.Info, $"String lines: {entries.Count}");
        Logger.Log<StringsFileCompressor>(Logger.Level.Info, $"Write on file: {_ztrFile}");

        byte[] result = ztrFileHandler.Compressor(ztr, entries);
        await File.WriteAllBytesAsync(ztr, result);

        using (var tmp = new TempFile())
        {
            var ztrReader = new ZtrFileReader();
            if (tmp.Path != null)
            {
                await ztrReader.ZtrTextReader(FullPath.FromPath(ztr), tmp.Path);
                _stream.Name.Check(tmp.Path);
            }
        }

        if (!TaskDialogs.Skip)
        {
            new TaskDialogs().ShowTaskDialog("Task completed!", $"File {Path.GetFileNameWithoutExtension(ztr)} Wrote!", "Open folder in file manager?", ztr);
        }
    }

    /// <summary>
    /// Compress strings file using ZtrCompressor and Pulse euro encoding
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    //public void StringsToZtrPulseEncodingNewCompression(FileStream fs, string file)
    public void StringsToZtrPulseEncodingNewCompression()
    {
        ZtrFileHandler ztrFileHandler = new();
        string ztr = Path.ChangeExtension(_ztrFile, ".ztr");
        if (!File.Exists(ztr))
        {
            ztr = Dialogs.GetFile("Get Ztr File", "Ztr File|*.ztr") ?? throw new FileNotFoundException();
        }
        ZtrTextReader reader = new(_stream, StringsZtrFormatter.Instance);
        Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal);
        byte[] result = ztrFileHandler.PulseEncodingCompressor(ztr, entries);
        File.WriteAllBytes(ztr, result);

        using var tmp = new TempFile();
        string temp = tmp.Path ?? string.Empty;
        var ztrReader = new ZtrFileReader();

        if (temp == null) throw new ArgumentNullException();

        ztrReader.ZtrTextReader(FullPath.FromPath(ztr), temp);
        _stream.Name.Check(temp);
    }
    /// <summary>
    /// Use Pulse fake compression
    /// </summary>
    public void PackStringsWithPulse()
    {
        using FileStream output = File.Create(Path.ChangeExtension(_ztrFile, ".ztr"));
        ZtrTextReader reader = new(_stream, StringsZtrFormatter.Instance);
        ZtrFileEntry[] entries = reader.Read(out string name);

        ZtrFilePacker packer = new(output, FFXIIITextEncodingFactory.CreateEuro(), ZtrFileType.BigEndianCompressedDictionary);
        packer.Pack(entries);

        PulseLog();
        if (!TaskDialogs.Skip)
            new TaskDialogs().ShowTaskDialog("Task completed!", "Ztr file Writed!", "Open folder in file manager?", output.Name);
    }

    private static void PulseLog()
    {
        var log = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Pulse.log"));
        if (!log.Exists) return;
        TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
        if (log.LastWriteTime.Second <= now.Second - 30) return;
        Process.Start("NotePad.exe", Path.Combine(Environment.CurrentDirectory, "Pulse.log"));
    }
}