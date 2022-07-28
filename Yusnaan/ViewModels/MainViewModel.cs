using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meziantou.Framework;
using SimpleLogger;
using Yusnaan.Common;
using Yusnaan.Model;
using Yusnaan.Model.Extractors;
using Yusnaan.Model.Injectors;
using Yusnaan.Model.ztr;

namespace Yusnaan.ViewModels;

public class MainViewModel : ObservableObject
{
    private string OldPath { get; set; } = string.Empty;

    public MainViewModel()
    {
        StringsToWpdCompressorCommand = new AsyncRelayCommand(StringsToWpdCompressor);
        WpdToStringsDecompressorCommand = new AsyncRelayCommand(WpdToStringsDecompressor);
        AllZtrCompressorCommand = new AsyncRelayCommand(AllZtrCompressor);
        ZtrCompressorCommand = new AsyncRelayCommand(ZtrCompressor);
        AllZtrDecompressorCommand = new AsyncRelayCommand(AllZtrDecompressor);
        ZtrDecompressorCommand = new AsyncRelayCommand(ZtrDecompressor);
    }

    public ICommand StringsToWpdCompressorCommand { get; }
    public ICommand WpdToStringsDecompressorCommand { get; }
    public ICommand AllZtrCompressorCommand { get; }
    public ICommand ZtrCompressorCommand { get; }
    public ICommand AllZtrDecompressorCommand { get; }
    public ICommand ZtrDecompressorCommand { get; }

    private async Task StringsToWpdCompressor()
    {
        try
        {
            FileInfo[]? wpdFile = Dialogs.TestGetScenarioFiles(OldPath);
            if (wpdFile == null) return;
            string? stringsFolder = Dialogs.ShowFolderBrowserDialog("Set strings files directories.");
            if (stringsFolder == null) return;

            foreach (FileInfo f in wpdFile)
            {
                Logger.Log(Logger.Level.Info, $"Wpd files count: {wpdFile.Length}");

                OldPath = f.DirectoryName ?? string.Empty;
                
                var wpdUnpack = new WpdZtrUnpack();
                wpdUnpack.ZtrEntryExtract(f, out FullPath? fileName);
                if (fileName == null) continue;

                string? stringsFile =
                    Dialogs.CheckStringsFile($"{stringsFolder}\\{fileName.Value.NameWithoutExtension}.strings");
                
                if (stringsFile == null)
                {
                    new TaskDialogs().ShowSkipDialog("File not founded!", "It's was skipped.", $"Strings file not founded for {f.Name}!");
                    Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"Strings file not founded for {f.Name}!");
                    continue;
                }

                FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                await using (fileStream.ConfigureAwait(false))
                {
                    var writer = new ZtrFileCompressorWriter(fileStream, fileName.Value.Value);
                    await writer.PackStringsNewCompression();

                var wpdInjector = new WpdEntryInjector();
                wpdInjector.TestPack(f);
                
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
            wpdUnpack.ZtrEntryExtract(wpdFile, out FullPath? fileName);
                
            var reader = new ZtrFileReader();
            await Task.Run(() => reader.ToStrings(fileName ?? throw new InvalidOperationException())).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task ZtrCompressor()
    {
        try
        {
            string? stringsFile = Dialogs.GetFile("Get strings file!", "Strings File|*.strings");
            if (stringsFile == null) return;
            await using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var writer = new ZtrFileCompressorWriter(fileStream, stringsFile);
            await writer.PackStringsNewCompression();
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task AllZtrCompressor()
    {
        try
        {
            string? folder = Dialogs.ShowFolderBrowserDialog("Target strings folder");
            if (folder == null) return;

            foreach (string f in Directory.EnumerateFiles(folder, "*.strings", SearchOption.AllDirectories))
            {
                await using FileStream fileStream = new(f, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var writer = new ZtrFileCompressorWriter(fileStream, f);
                await writer.PackStringsNewCompression();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task ZtrDecompressor()
    {
        try
        {
            FullPath? ztrFile = Dialogs.GetFile("Ztr file to decompress", "Ztr File|*.ztr");
            if (ztrFile == null) return;

            var reader = new ZtrFileReader();
            await reader.ToStrings(ztrFile);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task AllZtrDecompressor()
    {
        string? folder = Dialogs.ShowFolderBrowserDialog("Target ztr folder");
        if (folder == null) return;
        IEnumerable<FileInfo> files = new DirectoryInfo(folder).EnumerateFiles("*.ztr", SearchOption.AllDirectories);
        foreach (FileInfo fileName in files)
        {
            await using FileStream fileStream = fileName.OpenRead();
            var reader = new ZtrFileReader();
            await reader.ZtrTextReader(fileName);
        }
    }
}