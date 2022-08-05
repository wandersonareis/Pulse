using System.Windows.Input;
using Meziantou.Framework;
using SimpleLogger;
using Yusnaan.Model.ztr;

namespace Yusnaan.ViewModels;

public class ZtrDecompressorViewModel
{
    public string ZtrDecompressorContent => "Ztr to Strings";
    public string AllZtrDecompressorContent => "All Ztr to Strings";
    public string ZtrDecompressorTooltips => "Extracts strings file from ztr file format using ZtrDecompressor tool.";
    public string AllZtrDecompressorTooltips => "Extracts strings file from all ztr file format in directory and all its subdirectories using ZtrDecompressor tool.";

    public ZtrDecompressorViewModel()
    {
        AllZtrDecompressorCommand = new AsyncRelayCommand(AllZtrDecompressor);
        ZtrDecompressorCommand = new AsyncRelayCommand(ZtrDecompressor);
    }
    
    public ICommand AllZtrDecompressorCommand { get; }
    public ICommand ZtrDecompressorCommand { get; }
    
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