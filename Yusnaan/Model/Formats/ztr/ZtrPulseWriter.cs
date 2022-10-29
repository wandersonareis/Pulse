using System.Diagnostics;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;

namespace Yusnaan.Model.ztr;

internal class ZtrPulseWriter
{
    private readonly FileStream _stream;
    private readonly string _file;

    public ZtrPulseWriter(){}

    public ZtrPulseWriter(FileStream fs, string fi)
    {
        _stream = fs;
        _file = fi;
    }

    public void PackStringsWithPulse()
    {
        using FileStream output = File.Create(Path.ChangeExtension(_file, ".ztr"));
        ZtrTextReader reader = new(_stream, StringsZtrFormatter.Instance);
        ZtrFileEntry[] entries = reader.Read(out string name);

        ZtrFilePacker packer = new(output, FFXIIITextEncodingFactory.CreateEuro(), ZtrFileType.BigEndianCompressedDictionary);
        packer.Pack(entries);
        PulseLog();
        if (!TaskDialogs.Skip)
            new TaskDialogs().ShowTaskDialog("Task completed!", "Ztr file Writed!", "Open folder in file manager?", output.Name);
    }

    public void PackAllStringsWithPulse(FileInfo stringsFile)
    {
        try
        {
            using FileStream output = File.Create(Path.ChangeExtension(stringsFile.FullName, ".ztr"));
            ZtrTextReader reader = new(stringsFile.OpenRead(), StringsZtrFormatter.Instance);
            ZtrFileEntry[] entries = reader.Read(out string name);

            ZtrFilePacker packer = new(output, FFXIIITextEncodingFactory.CreateEuro(),
                ZtrFileType.BigEndianCompressedDictionary);
            packer.Pack(entries);
            PulseLog();
            if (!TaskDialogs.Skip)
                new TaskDialogs().ShowTaskDialog("Task completed!", "Ztr file Writed!", "Open folder in file manager?",
                    output.Name);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
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