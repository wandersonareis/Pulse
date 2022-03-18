using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.FS;
using Pulse.Gui.Common;
using Pulse.Gui.Compressor;
using static System.IO.Directory;

namespace Pulse.Gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Utils utils = new();

        private byte[] info4 = new byte[4];

        /// <summary>
        /// Unpack Ztr button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZtrUnpack_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
            if (openFileDialog.FileName == null) return;

            string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            string name = fileName?.Split((char)46).First();
            string lang = name?[^3..];

            if (!GameEncoding.EncodingCode.TryGetValue(lang ?? string.Empty, out int encodingCode)) encodingCode = 65001;

            ZtrToStrings.ToStrings(openFileDialog.FileName, fileName, encodingCode);

        }
        private void AllZtrUnpack_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
            foreach (var (f, fileName, lang) in from string f in EnumerateFiles(openFileDialog.SelectedPath, "*.ztr", SearchOption.AllDirectories)
                                                let fileName = Path.GetFileNameWithoutExtension(f)
                                                let ext = Path.GetExtension(f)
                                                let name = fileName?.Split((char)46).First()
                                                let lang = name[^3..]
                                                select (f, fileName, lang))
            {
                if (!GameEncoding.EncodingCode.TryGetValue(lang, out int encodingCode)) encodingCode = 65001;
                using FileStream fileStream = new(f, FileMode.Open, FileAccess.Read);
                ZtrToStrings.ToStrings(f, fileName, encodingCode);
            }
        }
        private void ButtonWpdUnpack_Click(object sender, EventArgs e)
        {
            string wpdFile = Dialogs.GetFile("Get Scenario File", "Scenare file|*.bin");
            if (wpdFile == null) return;

            WpdZtrUnpack.Extract(wpdFile);
        }
        
        private void ButtonAllWpdUnpack_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new();
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel) return;
            string[] array = EnumerateFiles(folderBrowserDialog.SelectedPath, "*_us.bin", SearchOption.AllDirectories).OrderBy(f => f).ToArray();
            foreach (string v in array)
            {
                WpdZtrUnpack.Extract(v);
            }
        }

        public void Extract(FileStream fs, FileInfo fi, int encoding = 65001)
        {
            ZtrFileHandler unpack = new();
            //ZtrFileUnpacker unpacker = new ZtrFileUnpacker(fs, FFXIIITextEncodingFactory.CreateEuro());
            //var entries = unpacker.Unpack();
            Dictionary<string, string> entries = unpack.DecompressorDict(fi.FullName, encoding);

            using FileStream output = File.Create(Path.ChangeExtension(fi.FullName, ".strings"));
            ZtrTextWriter writer = new(output, StringsZtrFormatter.Instance);
            writer.Write(fi.FullName, entries);
            //writer.Write(fi.FullName, entries);
        }

        private void StringsPack_Click(object sender, EventArgs e)
        {
            string stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile is null) return;
            using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Packer.PackStringsNewCompression(fileStream, stringsFile);

        }
        private void PackPulseStrings_Click(object sender, EventArgs e)
        {
            string stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile is null) return;
            using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Packer.PackStringsWithPulse(fileStream, stringsFile);
        }
        private void AllStringsNewPack_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;

            foreach (string f in EnumerateFiles(openFileDialog.SelectedPath, "*.strings", SearchOption.AllDirectories))
            {
                using FileStream fileStream = new(f, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                Packer.PackStringsNewCompression(fileStream, f);
            }
        }
        

        private void ButtonPackWpd_Click(object sender, EventArgs e)
        {
            string wpdFile = Dialogs.GetFile("Get Scenario File", "Scenare file|*.bin");
            if (wpdFile == null) return;

            WpdZtrPacker.Pack(wpdFile);

            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //if (folderBrowserDialog.ShowDialog() != DialogResult.Cancel)
            //{
            //    WpdUnpack fs = new WpdUnpack();
            //    fs.repackWPDGlobalAlignment(folderBrowserDialog.SelectedPath, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
            //}
            //using (OpenFileDialog openFileDialog = new())
            //{
            //    if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            //    {
            //        FileInfo fileInfo = new(openFileDialog.FileName);
            //        WpdUnpack fs = new();
            //        fs.RepackWpdGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value,
            //            (uint)numericUpDown3.Value);
            //    }
            //}
        }
        #region old Things
        private void button6_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
            FileInfo fileInfo = new(openFileDialog.FileName);
            //File.Delete(openFileDialog.FileName);
            //FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
            string readAllText = File.ReadAllText(openFileDialog.FileName);
            Acentos(readAllText, ref fileInfo);
        }

        private Dictionary<string, string> gamecode = new()
        {
            {"À", "{Var85 159}"},
            {"Á", "{Var85 160}"},
            {"Ã", "{Var85 162}"},
            {"Ä", "{Var85 163}"},
            {"—", "{Var85 87}"},
            {"È", "{Var85 167}"},
            {"É", "{Var85 168}"},
            {"Ì", "{Var85 171}"},
            {"Í", "{Var85 172}"},
            {"Ò", "{Var85 177}"},
            {"Ó", "{Var85 178}"},
            {"Ô", "{Var85 179}"},
            {"Õ", "{Var85 180}"},
            {"Ù", "{Var85 184}"},
            {"Ú", "{Var85 185}"},
            {"Ç", "{Var85 166}"}
        };
        public static string Replace(string sb,
            Dictionary<string, string> dict)
        {
            foreach (string s in dict.Keys)
            {
                sb = sb.Replace(s, dict[s]);
            }

            return sb;
        }
        public void Acentos(string text, ref FileInfo fi)
        {
            string novoConteudoTxt = Replace(text, gamecode);
            //var novoConteudoTxt = text.Replace("À", "{Var85 159}")
            //    .Replace("Á", "{Var85 160}")
            //    .Replace("Â", "{Var85 161}")
            //    .Replace("Ã", "{Var85 162}")
            //    .Replace("Ä", "{Var85 163}")
            //    .Replace("—", "{Var85 87}")
            //    .Replace("È", "{Var85 167}")
            //    .Replace("É", "{Var85 168}")
            //    .Replace("Ê", "{Var85 169}")
            //    .Replace("Ì", "{Var85 171}")
            //    .Replace("Í", "{Var85 172}")
            //    .Replace("Ò", "{Var85 177}")
            //    .Replace("Ó", "{Var85 178}")
            //    .Replace("Ô", "{Var85 179}")
            //    .Replace("Õ", "{Var85 180}")
            //    .Replace("Ù", "{Var85 184}")
            //    .Replace("Ú", "{Var85 185}")
            //    .Replace("Ç", "{Var85 166}");

            //###### Salva o txt com os valores substituídos onde o usuário escolher ######//
            string s = Path.ChangeExtension(fi.FullName, ".string");
            File.WriteAllText(s, novoConteudoTxt, Encoding.UTF8);
            FileStream fileStream = new(s, FileMode.Open, FileAccess.ReadWrite);
            Packer.PackStringsWithPulse(fileStream, s);
            //File.Delete(fi.FullName);
            //File.Delete(s);
            //var w = new WpdUnpack();
            //w.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
        }
        #endregion

        private void button10_Click(object sender, EventArgs e)
        {
            string ztrFile = Dialogs.GetFile("Get Ztr File", "Ztr file|*.ztr");
            if (ztrFile == null) return;

            using Stream input = File.OpenRead(ztrFile);
            using Stream output = File.Create(Path.ChangeExtension(ztrFile, ".strings"));

            ZtrToStrings.PulseUnpack(ztrFile, input, output);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string stringsFile = Dialogs.GetFile("Get strings file!");
            if (stringsFile is null) return;
            using FileStream fileStream = new(stringsFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            Packer.StringsToZtrPulseEncodingNewCompression(fileStream, stringsFile);
        }
    }
}