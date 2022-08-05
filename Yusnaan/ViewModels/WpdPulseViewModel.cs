using System.Linq;
using System.Windows.Input;
using SimpleLogger;
using Yusnaan.Model.Extractors;
using Yusnaan.Model.Injectors;

namespace Yusnaan.ViewModels;

public class WpdPulseViewModel
{
    public string WpdUnpackContent => "Wpd Unpack";
    public string AllWpdUnpackContent => "All Wpd Unpack";
    public string WpdPackContent => "Wpd Repack";
    public string WpdUnpackTooltips => "Extracts only ztr file from wpd file using Pulse tool.";
    public string AllWpdUnpackTooltips => "Extracts only ztr file from all wpd file in directory and all its subdirectories using Pulse tool.";
    public string WpdPackTooltips => "Inserts only the ztr file into a wpd file using the Pulse tool.";
    
    public WpdPulseViewModel()
    {
        PulseWpdPackCommand = new AsyncRelayCommand(PulseWpdPack);
        PulseWpdUnpackCommand = new AsyncRelayCommand(PulseWpdUnpack);
        PulseAllWpdUnpackCommand = new AsyncRelayCommand(PulseAllWpdUnpack);
    }
    
    public ICommand PulseAllWpdUnpackCommand { get; }
    public ICommand PulseWpdUnpackCommand { get; }
    public ICommand PulseWpdPackCommand { get; }
    
    private async Task PulseWpdPack()
    {
        try
        {
            FileInfo? wpdFile = Dialogs.TestGetScenarioFile();
            if (wpdFile == null) return;
        
            var wpdInjector = new WpdEntryInjector();
            await wpdInjector.PackAsync(wpdFile);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    
    private async Task PulseWpdUnpack()
    {
        try
        {
            FileInfo? wpdFile = Dialogs.TestGetScenarioFile();
            if (wpdFile == null) return;

            var wpdUnpack = new WpdZtrUnpack();
            await wpdUnpack.ZtrEntryExtractAsync(wpdFile);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task PulseAllWpdUnpack()
    {
        try
        {
            string? folder = Dialogs.ShowFolderBrowserDialog("Target wpd(bin) folder");
            if (folder == null) return;
            IOrderedEnumerable<FileInfo> files = new DirectoryInfo(folder).EnumerateFiles("*_us.bin", SearchOption.AllDirectories)
                .OrderBy(f => f);
            //FileInfo[] array = Directory.EnumerateFiles(folder, "*_us.bin", SearchOption.AllDirectories).Select(x => new FileInfo(x)).OrderBy(f => f).ToArray();
            var wpdUnpack = new WpdZtrUnpack();
            foreach (FileInfo wpdFile in files)
            {
                await wpdUnpack.ZtrEntryExtractAsync(wpdFile);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}