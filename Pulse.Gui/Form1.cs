using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.FS;
using Pulse.UI;

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

        private byte[] wpdEntryNameBuffer = new byte[16];

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                extract(ref fileStream, ref fileInfo);
                fileStream.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                WpdUnpack fs = new WpdUnpack();
                fs.unpackWpd(ref fileStream, ref fileInfo);
                fileStream.Close();
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.Cancel)
            {
                string[] array = (from f in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories)
                                  orderby f
                                  select f).ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    FileInfo fileInfo = new FileInfo(array[i]);
                    FileStream fileStream = new FileStream(array[i], FileMode.Open, FileAccess.Read);
                    WpdUnpack fs = new WpdUnpack();
                    fs.unpackWpd(ref fileStream, ref fileInfo);
                    fileStream.Close();
                }
            }
        }

        public void extract(ref FileStream fs, ref FileInfo fi)
        {
            using (var Output = File.Create(Path.ChangeExtension(fi.FullName, ".strings")))
            {
                ZtrFileUnpacker unpacker = new ZtrFileUnpacker(fs, InteractionService.TextEncoding.Provide().Encoding);
                ZtrFileEntry[] entries = unpacker.Unpack();

                ZtrTextWriter writer = new ZtrTextWriter(Output, StringsZtrFormatter.Instance);
                writer.Write(fi.FullName, entries);
            }
        }
                
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                pack(ref fileStream, ref fileInfo);
                fileStream.Close();
            }
        }
        public void pack(ref FileStream fs, ref FileInfo fi)
        {
            ZtrFileType type = ZtrFileType.BigEndianCompressedDictionary;
            if (fi.FullName.Contains("txtres\\resident\\system\\txtres_"))            
                type = ZtrFileType.BigEndianCompressedDictionary;           

            using (var Output = File.Create(Path.ChangeExtension(fi.FullName, ".ztr")))
            {                
                    string name;
                    ZtrTextReader reader = new ZtrTextReader(fs, StringsZtrFormatter.Instance);
                    ZtrFileEntry[] entries = reader.Read(out name);

                    ZtrFilePacker packer = new ZtrFilePacker(Output, InteractionService.TextEncoding.Provide().Encoding, type);
                    packer.Pack(entries);
                    Output.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //if (folderBrowserDialog.ShowDialog() != DialogResult.Cancel)
            //{
            //    WpdUnpack fs = new WpdUnpack();
            //    fs.repackWPDGlobalAlignment(folderBrowserDialog.SelectedPath, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
            //}
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);                
                WpdUnpack fs = new WpdUnpack();
                fs.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                string readAllText;
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                //File.Delete(openFileDialog.FileName);
                //FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                readAllText = File.ReadAllText(openFileDialog.FileName);
                acentos(readAllText, ref fileInfo);
            }
        }
        public void acentos(string fs, ref FileInfo fi)
        {
            string novoConteudoTxt;

            novoConteudoTxt = fs.Replace("À", "{Var85 159}").Replace("Á", "{Var85 160}").Replace("Â", "{Var85 161}").Replace("Ã", "{Var85 162}").Replace("Ä", "{Var85 163}")
                                            .Replace("—", "{Var85 87}").Replace("È", "{Var85 167}").Replace("É", "{Var85 168}").Replace("Ê", "{Var85 169}").Replace("Ì", "{Var85 171}")
                                            .Replace("Í", "{Var85 172}").Replace("Ò", "{Var85 177}").Replace("Ó", "{Var85 178}").Replace("Ô", "{Var85 179}").Replace("Õ", "{Var85 180}")
                                            .Replace("Ù", "{Var85 184}").Replace("Ú", "{Var85 185}").Replace("Ç", "{Var85 166}");

            //###### Salva o txt com os valores substituídos onde o usuário escolher ######//
            string s = Path.ChangeExtension(fi.FullName, ".string");
            File.WriteAllText(s, novoConteudoTxt, Encoding.UTF8);
            FileInfo fileInfo = new FileInfo(s);
            string strings = fileInfo.DirectoryName + "\\" + fi.Name;
            FileStream fileStream = new FileStream(s, FileMode.Open, FileAccess.ReadWrite);
            pack(ref fileStream, ref fileInfo);
            fileStream.Close();
            File.Delete(fi.FullName);
            File.Delete(s);
            WpdUnpack w = new WpdUnpack();
            w.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
        }
    }
}
