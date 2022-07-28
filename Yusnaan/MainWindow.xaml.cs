using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using Pulse.UI;
using SimpleLogger.Logging.Handlers;
using SimpleLogger;
using Yusnaan.Common;
using Yusnaan.Formats.imgb;
using Yusnaan.Model.Extractors;
using Yusnaan.Model.Injectors;
using Yusnaan.Model.ztr;

namespace Yusnaan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public FFXIIIGamePart Result { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Logger.LoggerHandlerManager
            .AddHandler(new FileLoggerHandler());
        }

        #region Pulse
        private void ZtrPulseCompressor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? stringsFile = Dialogs.GetFile("Get strings file!");
                if (stringsFile is null) return;
                using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var writer = new ZtrPulseWriter(fileStream, stringsFile);
                writer.PackStringsWithPulse();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }
        private void PulseZtrUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? ztrFile = Dialogs.GetFile("Get Ztr File", "Ztr file|*.ztr");
                if (ztrFile == null) return;

                using Stream input = File.OpenRead(ztrFile);
                using Stream output = File.Create(Path.ChangeExtension(ztrFile, ".strings"));

                var reader = new ZtrFileReader();
                reader.PulseUnpack(ztrFile, input, output);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        private void PulseWpdUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileInfo? wpdFile = Dialogs.TestGetScenarioFile();
                if (wpdFile == null) return;

                var wpdUnpack = new WpdZtrUnpack();
                wpdUnpack.ExtractInfo(wpdFile);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }
        private void PulseWpdPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileInfo? wpdFile = Dialogs.TestGetScenarioFile();
                if (wpdFile == null) return;
                
                var wpdInjector = new WpdEntryInjector();
                wpdInjector.TestPack(wpdFile);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }
        private void PulseAllZtrUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? folder = Dialogs.ShowFolderBrowserDialog("Target ztr folder");
                if (folder == null) return;
                foreach (string f in Directory.EnumerateFiles(folder, "*.ztr", SearchOption.AllDirectories))
                {
                    using Stream input = File.OpenRead(f);
                    using Stream output = File.Create(Path.ChangeExtension(f, ".strings"));

                    var reader = new ZtrFileReader();
                    reader.PulseUnpack(f, input, output);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        private void PulseAllWpdUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? folder = Dialogs.ShowFolderBrowserDialog("Target wpd(bin) folder");
                if (folder == null) return;
                IOrderedEnumerable<FileInfo> files = new DirectoryInfo(folder).EnumerateFiles("*_us.bin", SearchOption.AllDirectories)
                    .OrderBy(f => f);
                //FileInfo[] array = Directory.EnumerateFiles(folder, "*_us.bin", SearchOption.AllDirectories).Select(x => new FileInfo(x)).OrderBy(f => f).ToArray();
                var wpdUnpack = new WpdZtrUnpack();
                foreach (FileInfo v in files)
                {
                    wpdUnpack.ExtractInfo(v);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }
        #endregion

        #region PulseButton

        private void OnPart1ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Pulse", "-ff13");
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }
        private void OnPart2ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Pulse", "-ff132");
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }
        private void OnPart3ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Pulse", "-ff133");
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(1);
        }

        [SuppressMessage("ReSharper", "UnusedVariable")]
        private void UnpackXvf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
                if (xfvFile == null) return;
                string unpackPath = Path.Combine(Path.GetDirectoryName(xfvFile)!, $"{xfvFile.GetFileNameMultiDotExtension()}.unpack");
                Directory.CreateDirectory(unpackPath);
                using Stream xfvFileStream = File.OpenRead(xfvFile);
                using Stream imgbFileStream = File.OpenRead(Path.ChangeExtension(xfvFile, ".imgb"));
                var wdbHeader = xfvFileStream.ReadContent<WdbHeader>();
                var buff = new byte[32 * 1024];

                foreach (WpdEntry entry in wdbHeader.Entries.Where(x => string.Equals(x.Extension, "vtex", StringComparison.Ordinal)))
                {
                    xfvFileStream.Position = entry.Offset;

                    var sectionHeader = xfvFileStream.ReadContent<SectionHeader>();
                    var textureHeader = xfvFileStream.ReadContent<VtexHeader>();
                    xfvFileStream.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
                    var gtex = xfvFileStream.ReadContent<GtexData>();
                    using Stream output = File.Create(Path.Combine(unpackPath, textureHeader.Name + ".dds"));

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
        
        private void PackXvf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? ddsFile = Dialogs.GetFile("Get Dds File", "dds file|*.dds");
                if (ddsFile == null) return;
                FileStream input = File.OpenRead(ddsFile);
                string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
                if (xfvFile == null) return;
                using Stream xfvFileStream = File.Open(xfvFile, FileMode.Open, FileAccess.ReadWrite);
                using Stream imgbFileStream = File.Open(Path.ChangeExtension(xfvFile, ".imgb"), FileMode.Open, FileAccess.ReadWrite);

                var xfv = new XfvInject(Path.GetFileNameWithoutExtension(ddsFile),input, xfvFileStream, imgbFileStream);
                xfv.Inject();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        private void PackAllXfv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? ddsPath = Dialogs.ShowFolderBrowserDialog("Target dds folder");
                if (ddsPath == null) return;
                string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
                if (xfvFile == null) return;
                using Stream xfvFileStream = File.Open(xfvFile, FileMode.Open, FileAccess.ReadWrite);
                using Stream imgbFileStream = File.Open(Path.ChangeExtension(xfvFile, ".imgb"), FileMode.Open,
                    FileAccess.ReadWrite);

                var wdbHeader = xfvFileStream.ReadContent<WdbHeader>();
                var buff = new byte[32 * 1024];

                foreach (WpdEntry entry in wdbHeader.Entries)
                {
                    string? ddsFile = Directory.GetFiles(ddsPath, entry.NameWithoutExtension + ".dds").FirstOrDefault();
                    if (ddsFile == null) continue;
                    FileStream input = File.OpenRead(ddsFile);

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
}
