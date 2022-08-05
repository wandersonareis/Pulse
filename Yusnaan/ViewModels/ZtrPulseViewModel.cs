using System.Windows.Input;
using SimpleLogger;
using Yusnaan.Model.ztr;

namespace Yusnaan.ViewModels;

public class ZtrPulseViewModel
{
    public string PulseZtrUnpackContent => "Unpack Ztr";
    public string PulseAllZtrUnpackContent => "Unpack All Ztr";
    public string PulseZtrPackContent => "Repack Strings";
    public string PulseZtrUnpackTooltips => "Extracts strings file from ztr file using Pulse tool.";
    public string PulseAllZtrUnpackTooltips => "Extracts strings file from all ztr file in directory and all its subdirectories using Pulse tool.";
    public string PulseZtrPackTooltips => "Repack strings file into ztr file using the Pulse tool fake compression big endian.";

    public ZtrPulseViewModel()
    {
        ZtrPulsePackCommand = new AsyncRelayCommand(ZtrPulsePack);
        ZtrPulseUnpackCommand = new AsyncRelayCommand(PulseZtrUnpack);
        AllZtrPulseUnpackCommand = new AsyncRelayCommand(PulseAllZtrUnpack);
    }
    
    public ICommand AllZtrPulseUnpackCommand { get; }
    public ICommand ZtrPulseUnpackCommand { get; }
    public ICommand ZtrPulsePackCommand { get; }
    
    private async Task PulseZtrUnpack()
    {
        try
        {
            string? ztrFile = Dialogs.GetFile("Get Ztr File", "Ztr file|*.ztr");
            if (ztrFile == null) return;

            await using Stream input = File.OpenRead(ztrFile);
            await using Stream output = File.Create(Path.ChangeExtension(ztrFile, ".strings"));

            var reader = new ZtrFileReader(input, output);
            reader.PulseUnpack(ztrFile);
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task PulseAllZtrUnpack()
    {
        try
        {
            string? folder = Dialogs.ShowFolderBrowserDialog("Target ztr folder");
            if (folder == null) return;
            foreach (string f in Directory.EnumerateFiles(folder, "*.ztr", SearchOption.AllDirectories))
            {
                await using Stream input = File.OpenRead(f);
                await using Stream output = File.Create(Path.ChangeExtension(f, ".strings"));

                var reader = new ZtrFileReader(input, output);
                reader.PulseUnpack(f);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task ZtrPulsePack()
    {
        try
        {
            string? stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile is null) return;
            await using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var writer = new ZtrPulseWriter(fileStream, stringsFile);
            writer.PackStringsWithPulse();
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}