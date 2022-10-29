using System.Windows.Input;
using SimpleLogger;
using Yusnaan.Model.ztr;

namespace Yusnaan.ViewModels;

public class ZtrCompressorViewModel
{
    public string ZtrCompressorContent => "Strings to Ztr";
    public string AllZtrCompressorContent => " All Strings to Ztr";
    public string ZtrCompressorTooltips => "Compress strings file into ztr file format using ZtrDecompressor tool.";
    public string AllZtrCompressorTooltips => "Compress all strings files into ztr file format from directory and all its subdirectories using ZtrDecompressor tool.";
    
    public ZtrCompressorViewModel()
    {
        AllZtrCompressorCommand = new AsyncRelayCommand(AllZtrCompressor);
        ZtrCompressorCommand = new AsyncRelayCommand(ZtrCompressor);
    }
    public ICommand AllZtrCompressorCommand { get; }
    public ICommand ZtrCompressorCommand { get; }
    
    private async Task ZtrCompressor()
    {
        try
        {
            string? stringsFile = Dialogs.GetFile("Get strings file!", "Strings File|*.strings");
            if (stringsFile == null) return;
            await using FileStream inputFileStream = new(stringsFile, FileEx.FileStreamInputOptions());
            var writer = new StringsFileCompressor(inputFileStream, stringsFile);
            //await writer.PackStringsNewCompression();
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

            foreach (string file in Directory.EnumerateFiles(folder, "*.strings", SearchOption.AllDirectories))
            {
                await using FileStream inputFileStream = new(file, FileEx.FileStreamInputOptions());
                var writer = new StringsFileCompressor(inputFileStream, file);
                await writer.PackStringsNewCompression();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}