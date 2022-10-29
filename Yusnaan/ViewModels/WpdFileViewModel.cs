using System.Linq;
using System.Windows;
using System.Windows.Input;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using Yusnaan.Common;
using Yusnaan.Model.Extractors;
using Yusnaan.Model.Injectors;
using Yusnaan.Model.ztr;
using FileEx = Yusnaan.Common.FileEx;

namespace Yusnaan.ViewModels;

internal class WpdFileViewModel
{
    private string OldPath { get; set; } = string.Empty;
    public string WpdToStrings => "Wpd to Strings";
    public string StringsToWpd => "Strings to Wpd";
    public string WpdToStringsTooltips => "Extracts strings file from wpd scenarie file using ZtrDecompressor tool.";
    public string StringsToWpdTooltips =>
        "Compress strings file in ztr file format and insert into wpd scenarie file using ZtrDecompressor tool.";

    public WpdFileViewModel()
    {
        StringsToWpdCompressorCommand = new AsyncRelayCommand(StringsToWpdCompressor);
        WpdToStringsDecompressorCommand = new AsyncRelayCommand(WpdToStringsDecompressor);
    }
    public ICommand StringsToWpdCompressorCommand { get; }
    public ICommand WpdToStringsDecompressorCommand { get; }

    
    private async Task StringsToWpdCompressor()
    {
        try
        {
            FileInfo[]? wpdFile = Dialogs.GetScenarioFilesAsync(OldPath);
            if (wpdFile == null) return;
            string? stringsFolder = Dialogs.ShowFolderBrowserDialog("Set strings files directories.");
            if (stringsFolder == null) return;

            foreach (FileInfo fileInfo in wpdFile)
            {
                Logger.Log(Logger.Level.Info, $"Wpd files count: {wpdFile.Length}");

                OldPath = fileInfo.DirectoryName ?? string.Empty;

                if (fileInfo.CheckDummyFile()) continue;

                WpdZtrUnpack wpdUnpack = new();
                FullPath ztrFileName = await wpdUnpack.ZtrEntryExtractAsync(fileInfo);

                if (string.IsNullOrEmpty(ztrFileName.Value))
                {
                    new TaskDialogs().ShowSkipDialog("It's was skipped.", $"Ztr file name not founded inside {fileInfo.FullName}!", "");
                    Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Ztr file name not founded inside {fileInfo.FullName}!");
                    continue;
                }

                string? stringsFile =
                    Dialogs.CheckStringsFile($"{stringsFolder}\\{ztrFileName.NameWithoutExtension}.strings");

                if (stringsFile == null)
                {
                    new TaskDialogs().ShowSkipDialog("File not founded!", "It's was skipped.",
                        $"Strings file not founded for {fileInfo.Name}!");
                    Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Strings file not founded for {fileInfo.Name}!");
                    continue;
                }

                FileStream inputFileStream = new(stringsFile, FileEx.FileStreamInputOptions());
                await using (inputFileStream.ConfigureAwait(false))
                {
                    var writer = new StringsFileCompressor(inputFileStream, ztrFileName.Value);

                    switch (ztrFileName.NameWithoutExtension)
                    {
                        case "auto_ded_us":
                        case "auto_lux_us":
                        case "auto_res_us":
                        case "auto_wil_us":
                        case "auto_yus_us":
                            await writer.PackStringsNewCompression();
                            break;
                        default:
                            writer.PackStringsWithPulse();
                            break;
                    }

                    //await writer.PackStringsNewCompression();

                    var wpdInjector = new WpdEntryInjector();
                    await wpdInjector.PackAsync(fileInfo);

                    Logger.Log("//////////////////////////////////////////////");
                }
            }
        }
        catch (Exception exception)
        {
            Logger.Log(exception);
            throw;
        }
    }
    private async Task WpdToStringsDecompressor()
    {
        try
        {
            FileInfo? wpdFile = Dialogs.TestGetScenarioFile();
            if (wpdFile == null) return;

            var wpdUnpack = new WpdZtrUnpack();
            FullPath ztrFileName = await wpdUnpack.ZtrEntryExtractAsync(wpdFile);
            
            if (string.IsNullOrEmpty(ztrFileName.Value))
            {
                new TaskDialogs().ShowSkipDialog("It's was skipped." , $"Ztr file name not founded inside {wpdFile.FullName}!", "");
                Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Ztr file name not founded inside {wpdFile.FullName}!");
                return;
            }
                
            var reader = new ZtrFileReader();
            await reader.ZtrTextReader(new FileInfo(ztrFileName.Value));
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}
sealed class TempFile : IDisposable
{
    string? _path;
    public TempFile() : this(System.IO.Path.GetTempFileName()) { }

    public TempFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
        _path = path;
    }
    public string? Path
    {
        get
        {
            if (_path == null) throw new ObjectDisposedException(GetType().Name);
            return _path;
        }
    }
    ~TempFile() { Dispose(false); }
    public void Dispose() { Dispose(true); }
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            GC.SuppressFinalize(this);
        }

        if (string.IsNullOrEmpty(_path)) return;

        try { File.Delete(_path); }
        catch { } // best effort
        _path = "";
    }
}