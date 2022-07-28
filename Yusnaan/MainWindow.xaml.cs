using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Be.IO;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using Pulse.UI;
using Yusnaan.Common;
using Yusnaan.Formats;
using Yusnaan.Formats.imgb;
using MessageBox = System.Windows.MessageBox;

namespace Yusnaan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public FFXIIIGamePart Result { get; set; }
        private string? OldPath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        #region ZtrDecompressor
        private void ZtrDecompressor_Click(object sender, RoutedEventArgs e)
        {
            string? ztrFile = Dialogs.GetFile("Ztr file to decompress", "Ztr File|*.ztr");
            if (ztrFile == null) return;

            ZtrToStrings.ToStrings(ztrFile);
        }
        private void ZtrCompressor_Click(object sender, RoutedEventArgs e)
        {
            string? stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile == null) return;
            using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Packer.PackStringsNewCompression(fileStream, stringsFile);
        }

        private void AllZtrDecompressor_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            foreach (var fileName in Directory.EnumerateFiles(openFileDialog.SelectedPath, "*.ztr", SearchOption.AllDirectories))
            {
                using FileStream fileStream = new(fileName, FileMode.Open, FileAccess.Read);
                ZtrToStrings.ToStrings(fileName);
            }
        }
        private void AllZtrCompressor_Click(object sender, RoutedEventArgs e)
        {
            using FolderBrowserDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            foreach (string f in Directory.EnumerateFiles(openFileDialog.SelectedPath, "*.strings", SearchOption.AllDirectories))
            {
                using FileStream fileStream = new(f, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                Packer.PackStringsNewCompression(fileStream, f);
            }
        }
        #endregion

        #region Pulse
        private void ZtrPulseCompressor_Click(object sender, RoutedEventArgs e)
        {
            string? stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile is null) return;
            using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Packer.PackStringsWithPulse(fileStream, stringsFile);
        }
        private void PulseZtrUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? ztrFile = Dialogs.GetFile("Get Ztr File", "Ztr file|*.ztr");
                if (ztrFile == null) return;

                using Stream input = File.OpenRead(ztrFile);
                using Stream output = File.Create(Path.ChangeExtension(ztrFile, ".strings"));

                ZtrToStrings.PulseUnpack(ztrFile, input, output);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }

        private void PulseWpdUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? wpdFile = Commons.GetScenarioFile();
                if (wpdFile == null) return;

                WpdZtrUnpack.Extract(wpdFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        private void WpdToStringsDecompressor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? wpdFile = Commons.GetScenarioFile();
                if (wpdFile == null) return;

                WpdZtrUnpack.Extract(wpdFile, out string? fileName);
                ZtrToStrings.ToStrings(fileName);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        private void StringsToWpdCompressor_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    string? wpdFile = Commons.GetScenarioFile(OldPath);
            //    if (wpdFile == null) return;
            //    OldPath = Path.GetDirectoryName(wpdFile);
            //    WpdZtrUnpack.Extract(wpdFile, out string? fileName);
            //    if (fileName == null) return;

            //    string? stringsFile = Commons.GetStringsFile(fileName);
            //    if (stringsFile == null) return;

            //    using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            //    Packer.PackStringsNewCompression(fileStream, fileName);

            //    ZtrToWpdEntryInjector.Pack(wpdFile);
            //}
            //catch (Exception ex)
            //{
            //    _ = MessageBox.Show(ex.Message);
            //    throw;
            //}
            var wpdFile = Commons.GetScenarioFiles(OldPath);
            if (wpdFile == null) return;
            var stringsFolder = Commons.GetFolder("Set strings files directories.");
            if (stringsFolder == null) return;

            foreach (string f in wpdFile)
            {
                OldPath = Path.GetDirectoryName(f);
                WpdZtrUnpack.Extract(f, out string? fileName);
                if (fileName == null) return;

                var noob = $"{stringsFolder}\\{Path.GetFileNameWithoutExtension(fileName)}.strings";
                string? stringsFile =
                    Commons.CheckStringsFile($"{stringsFolder}\\{Path.GetFileNameWithoutExtension(fileName)}.strings");
                if (stringsFile == null) return;

                using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                Packer.PackStringsNewCompression(fileStream, fileName);

                ZtrToWpdEntryInjector.Pack(f);
            }
        }
        private void PulseWpdPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? wpdFile = Commons.GetScenarioFile();

                if (wpdFile != null) ZtrToWpdEntryInjector.Pack(wpdFile);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        private void PulseAllZtrUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog openFileDialog = new();
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                foreach (string f in Directory.EnumerateFiles(openFileDialog.SelectedPath, "*.ztr", SearchOption.AllDirectories))
                {
                    using Stream input = File.OpenRead(f);
                    using Stream output = File.Create(Path.ChangeExtension(f, ".strings"));

                    ZtrToStrings.PulseUnpack(f, input, output);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }

        private void PulseAllWpdUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new();
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                string[] array = Directory.EnumerateFiles(folderBrowserDialog.SelectedPath, "*_us.bin", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
                foreach (string v in array)
                {
                    WpdZtrUnpack.Extract(v);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        #endregion

        private void OnPart1ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = FFXIIIGamePart.Part1;
                UiMainWindow main = new();

                InteractionService.SetGamePart(Result);
                main.Show();
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
                Result = FFXIIIGamePart.Part2;
                UiMainWindow main = new();

                InteractionService.SetGamePart(Result);
                main.Show();
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
                Result = FFXIIIGamePart.Part3;
                UiMainWindow main = new();

                InteractionService.SetGamePart(FFXIIIGamePart.Part3);
                main.Show();
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                Environment.Exit(1);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Environment.Exit(1);
        }

        private void unpackXvf_Click(object sender, RoutedEventArgs e)
        {
            string? xfvFile = Dialogs.GetFile("Get Vtex File", "xfv file|*.xfv");
            if (xfvFile == null) return;
            string unpackPath = Path.Combine(Path.GetDirectoryName(xfvFile) ?? throw new NullReferenceException(), Path.GetFileNameWithoutExtension(xfvFile), ".unpack");
            Directory.CreateDirectory(unpackPath);
            using Stream xfvFileStream = File.OpenRead(xfvFile);
            using Stream imgbFileStream = File.OpenRead(Path.ChangeExtension(xfvFile, ".imgb"));
            WdbHeader wdbHeader = xfvFileStream.ReadContent<WdbHeader>();
            byte[] buff = new byte[32 * 1024];

            foreach (WpdEntry entry in wdbHeader.Entries.Where(e => e.Extension == "vtex"))
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

        private void packXvf_Click(object sender, RoutedEventArgs e)
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

        private void packAllXfv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? ddsPath = Dialogs.GetFolder("DDS path");
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
            catch (NotImplementedException notImplementedException)
            {
                MessageBox.Show($"Algo não implementado!\n{notImplementedException.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
