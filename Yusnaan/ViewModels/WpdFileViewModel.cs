using System.Windows.Input;
using Meziantou.Framework;
using SimpleLogger;
using Yusnaan.Model.Extractors;
using Yusnaan.Model.Injectors;
using Yusnaan.Model.ztr;

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

            foreach (FileInfo f in wpdFile)
            {
                Logger.Log(Logger.Level.Info, $"Wpd files count: {wpdFile.Length}");

                OldPath = f.DirectoryName ?? string.Empty;
                
                var wpdUnpack = new WpdZtrUnpack();
                FullPath ztrFileName = await wpdUnpack.ZtrEntryExtractAsync(f);
                
                if (string.IsNullOrEmpty(ztrFileName.Value))
                {
                    new TaskDialogs().ShowSkipDialog("It's was skipped." , $"Ztr file name not founded inside {f.FullName}!", "");
                    Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Ztr file name not founded inside {f.FullName}!");
                    continue;
                }

                string? stringsFile =
                    Dialogs.CheckStringsFile($"{stringsFolder}\\{ztrFileName.NameWithoutExtension}.strings");
                
                if (stringsFile == null)
                {
                    new TaskDialogs().ShowSkipDialog("File not founded!", "It's was skipped.", $"Strings file not founded for {f.Name}!");
                    Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Strings file not founded for {f.Name}!");
                    continue;
                }

                FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                await using (fileStream.ConfigureAwait(false))
                {
                    var writer = new ZtrFileCompressorWriter(fileStream, ztrFileName.Value);
                    await writer.PackStringsNewCompression();

                    var wpdInjector = new WpdEntryInjector();
                    await wpdInjector.PackAsync(f);
                
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
            await Task.Run(() => reader.ToStrings(ztrFileName)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}