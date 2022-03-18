using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.FS;

namespace Pulse.Gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Utils utils = new Utils();

        private byte[] info4 = new byte[4];

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
            FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
            using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
            {
                Extract(fileStream, fileInfo);
            }
            //FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            //if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;

            //foreach (string f in Directory.EnumerateFiles(openFileDialog.SelectedPath, "*.ztr", SearchOption.AllDirectories))
            //{
            //    FileInfo fileInfo = new FileInfo(f);
            //    string fileName = Path.GetFileNameWithoutExtension(f);
            //    string ext = fileInfo.Extension;
            //    string name = fileName.Split((char)46).First();
            //    string lang = name.Substring(name.Length - 3);
            //    if (!GameEncoding._EncodingCode.TryGetValue(lang, out int encodingCode)) encodingCode = 65001;
            //    using (FileStream fileStream = new FileStream(f, FileMode.Open, FileAccess.Read))
            //    {
            //        Extract(fileStream, fileInfo, encodingCode);
            //    }
            //}
        }

        private void ButtonWpdUnpack_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
            FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
            FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
            WpdUnpack fs = new WpdUnpack();
            fs.UnpackWpd(fileStream, ref fileInfo);
            fileStream.Close();

        }
        private void ButtonAllWpdUnpack_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel) return;
            string[] array = (from f in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories)
                orderby f
                select f).ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(array[i]);
                using (FileStream fileStream = new FileStream(array[i], FileMode.Open, FileAccess.Read))
                {
                    WpdUnpack fs = new WpdUnpack();
                    fs.UnpackWpd(fileStream, ref fileInfo);
                    fileStream.Close();
                }

            }
        }

        public void Extract(FileStream fs, FileInfo fi, int encoding = 65001)
        {
            using (FileStream output = File.Create(Path.ChangeExtension(fi.FullName, ".string")))
            {
                ZtrFileHandler unpack = new ZtrFileHandler();
                //ZtrFileUnpacker unpacker = new ZtrFileUnpacker(fs, FFXIIITextEncodingFactory.CreateEuro());
                //var entries = unpacker.Unpack();
                Dictionary<string, string> entries = unpack.DecompressorDict(fi.FullName, encoding);

                ZtrTextWriter writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
                writer.Write(fi.FullName, entries);
                //writer.Write(fi.FullName, entries);
            }
        }

        private void ButtonZtrPack_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
                if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    using (FileStream fileStream =
                        new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        Pack(fileStream, fileInfo);
                    }

                }
            //using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
            //    if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            //    {
            //        foreach (string f in Directory.EnumerateFiles(openFileDialog.SelectedPath, "*.strings", SearchOption.AllDirectories))
            //        {
            //            FileInfo fileInfo = new FileInfo(f);
            //            using (FileStream fileStream =
            //                   new FileStream(f, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //            {
            //                Pack(fileStream, fileInfo);
            //            }
            //        }
            //    }
        }
        public void Pack(FileStream fs, FileInfo fi)
        {
            ZtrFileHandler ztrFileHandler = new ZtrFileHandler();
            string ztr = Path.ChangeExtension(fi.FullName, ".ztr");
            ZtrTextReader reader = new ZtrTextReader(fs, StringsZtrFormatter.Instance);
            Dictionary<string, string> entries = reader.Read(out string name).ToDictionary(e => e.Key, e => e.Value);
            byte[] result = ztrFileHandler.Compressor(ztr, entries);
            File.WriteAllBytes(ztr, result);

            //using (FileStream output = File.Create(Path.ChangeExtension(fi.FullName, ".ztr")))
            //{
            //    ZtrTextReader reader = new ZtrTextReader(fs, StringsZtrFormatter.Instance);
            //    ZtrFileEntry[] entries = reader.Read(out string name);

            //    ZtrFilePacker packer = new ZtrFilePacker(output, FFXIIITextEncodingFactory.CreateEuro(), type);
            //    packer.Pack(entries);
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //if (folderBrowserDialog.ShowDialog() != DialogResult.Cancel)
            //{
            //    WpdUnpack fs = new WpdUnpack();
            //    fs.repackWPDGlobalAlignment(folderBrowserDialog.SelectedPath, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
            //}
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    WpdUnpack fs = new WpdUnpack();
                    fs.RepackWpdGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value,
                        (uint)numericUpDown3.Value);
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.Cancel) return;
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                //File.Delete(openFileDialog.FileName);
                //FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                string readAllText = File.ReadAllText(openFileDialog.FileName);
                Acentos(readAllText, ref fileInfo);
            }
        }

        private Dictionary<string, string> gamecode = new Dictionary<string, string>()
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
            FileInfo fileInfo = new FileInfo(s);
            FileStream fileStream = new FileStream(s, FileMode.Open, FileAccess.ReadWrite);
            Pack(fileStream, fileInfo);
            //File.Delete(fi.FullName);
            //File.Delete(s);
            //var w = new WpdUnpack();
            //w.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
        }
    }
}