using System.Linq;
using System.Windows.Input;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using SimpleLogger;
using Yusnaan.Formats.imgb;

namespace Yusnaan.ViewModels;

public class VtexPulseViewModel
{
    public string VtexUnpackContent => "Vtex to DDS";
    public string AllVtexUnpackContent => "All DDS to Vtex";
    public string VtexPackContent => "DDS to Vtex";
    public string VtexUnpackTooltips => "Extracts all dds files from vtex file using Pulse tool.";
    public string AllVtexUnpackTooltips => "Inserts all dds files into vtex file using Pulse tool.";
    public string VtexPackTooltips => "Inserts selected dds file into vtex file using the Pulse tool.";
    
    public VtexPulseViewModel()
    {
        PulseVtexPackCommand = new AsyncRelayCommand(PackXvf);
        PulseVtexUnpackCommand = new AsyncRelayCommand(UnpackXvf);
        PulseAllDdsPackCommand = new AsyncRelayCommand(PackAllXfv);
    }
    
    public ICommand PulseVtexPackCommand { get; }
    public ICommand PulseVtexUnpackCommand { get; }
    public ICommand PulseAllDdsPackCommand { get; }
    
    private async Task UnpackXvf()
    {
        try
        {
            string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
            if (xfvFile == null) return;
            string unpackPath = Path.Combine(Path.GetDirectoryName(xfvFile)!, $"{xfvFile.GetFileNameMultiDotExtension()}.unpack");
            Directory.CreateDirectory(unpackPath);
            await using Stream xfvFileStream = File.OpenRead(xfvFile);
            await using Stream imgbFileStream = File.OpenRead(Path.ChangeExtension(xfvFile, ".imgb"));
            var wdbHeader = xfvFileStream.ReadContent<WdbHeader>();
            var buff = new byte[32 * 1024];

            foreach (WpdEntry entry in wdbHeader.Entries.Where(x => string.Equals(x.Extension, "vtex", StringComparison.Ordinal)))
            {
                xfvFileStream.Position = entry.Offset;

                var dummy = xfvFileStream.ReadContent<SectionHeader>();
                var textureHeader = xfvFileStream.ReadContent<VtexHeader>();
                xfvFileStream.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
                var gtex = xfvFileStream.ReadContent<GtexData>();
                await using Stream output = File.Create(Path.Combine(unpackPath, textureHeader.Name + ".dds"));

                DdsHeader header = DdsHeaderDecoder.FromGtexHeader(gtex.Header);
                DdsHeaderEncoder.ToFileStream(header, output);

                foreach (GtexMipMapLocation mipMap in gtex.MipMapData)
                {
                    imgbFileStream.Position = mipMap.Offset;
                    imgbFileStream.CopyToStream(output, mipMap.Length, buff);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task PackXvf()
    {
        try
        {
            string? ddsFile = Dialogs.GetFile("Get Dds File", "dds file|*.dds");
            if (ddsFile == null) return;
            FileStream input = File.OpenRead(ddsFile);
            string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
            if (xfvFile == null) return;
            await using Stream xfvFileStream = File.Open(xfvFile, FileMode.Open, FileAccess.ReadWrite);
            await using Stream imgbFileStream = File.Open(Path.ChangeExtension(xfvFile, ".imgb"), FileMode.Open, FileAccess.ReadWrite);

            var xfv = new XfvInject(Path.GetFileNameWithoutExtension(ddsFile),input, xfvFileStream, imgbFileStream);
            xfv.Inject();
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
    private async Task PackAllXfv()
    {
        try
        {
            string? ddsPath = Dialogs.ShowFolderBrowserDialog("Target dds folder");
            if (ddsPath == null) return;
            string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
            if (xfvFile == null) return;
            await using Stream xfvFileStream = File.Open(xfvFile, FileMode.Open, FileAccess.ReadWrite);
            await using Stream imgbFileStream = File.Open(Path.ChangeExtension(xfvFile, ".imgb"), FileMode.Open,
                FileAccess.ReadWrite);

            var wdbHeader = xfvFileStream.ReadContent<WdbHeader>();
            var buff = new byte[32 * 1024];

            foreach (WpdEntry entry in wdbHeader.Entries)
            {
                string? ddsFile = Directory.GetFiles(ddsPath, entry.NameWithoutExtension + ".dds").FirstOrDefault();
                if (ddsFile == null) continue;
                await using FileStream input = File.OpenRead(ddsFile);

                var xfv = new XfvInject(entry, input, xfvFileStream, imgbFileStream, buff);
                xfv.InjectAll();
                input.Close();
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ex);
            throw;
        }
    }
}