using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Pulse.Core;
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

        private byte[] wpdEntryNameBuffer = new byte[16];

        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                using (var fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    Extract(fileStream, fileInfo);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                var fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                var fs = new WpdUnpack();
                fs.unpackWpd(fileStream, ref fileInfo);
                fileStream.Close();
            }

        }
        private void button3_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() != DialogResult.Cancel)
            {
                var array = (from f in Directory.GetFiles(folderBrowserDialog.SelectedPath, "*", SearchOption.AllDirectories)
                                  orderby f
                                  select f).ToArray();
                for (var i = 0; i < array.Count(); i++)
                {
                    var fileInfo = new FileInfo(array[i]);
                    using (FileStream fileStream = new FileStream(array[i], FileMode.Open, FileAccess.Read))
                    {
                        var fs = new WpdUnpack();
                        fs.unpackWpd(fileStream, ref fileInfo);
                        fileStream.Close();
                    }
                        
                }
            }
        }

        public void Extract(FileStream fs, FileInfo fi)
        {
            using (var output = File.Create(Path.ChangeExtension(fi.FullName, ".strings")))
            {
                var unpacker = new ZtrFileUnpacker(fs, FFXIIITextEncodingFactory.CreateEuro());
                var entries = unpacker.Unpack();

                var writer = new ZtrTextWriter(output, StringsZtrFormatter.Instance);
                writer.Write(fi.FullName, entries);
            }
        }
                
        private void Button4_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
                if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    using (var fileStream =
                        new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        Pack(fileStream, fileInfo);
                    }
                        
                }
        }
        public void Pack(FileStream fs, FileInfo fi)
        {
            var type = ZtrFileType.LittleEndianUncompressedDictionary;
            if (fi.FullName.Contains("txtres\\resident\\system\\txtres_"))            
                type = ZtrFileType.BigEndianCompressedDictionary;           

            using (var output = File.Create(Path.ChangeExtension(fi.FullName, ".ztr")))
            {                
                    string name;
                    var reader = new ZtrTextReader(fs, StringsZtrFormatter.Instance);
                    var entries = reader.Read(out name);

                    var packer = new ZtrFilePacker(output, FFXIIITextEncodingFactory.CreateEuro(), type);
                    packer.Pack(entries);
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
            using (var openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    var fs = new WpdUnpack();
                    fs.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint) numericUpDown4.Value,
                        (uint) numericUpDown3.Value);
                }
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() != DialogResult.Cancel)
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    //File.Delete(openFileDialog.FileName);
                    //FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    var readAllText = File.ReadAllText(openFileDialog.FileName);
                    Acentos(readAllText, ref fileInfo);
                }
            }
        }
        public void Acentos(string fs, ref FileInfo fi)
        {
            var novoConteudoTxt = fs.Replace("À", "{Var85 159}").Replace("Á", "{Var85 160}").Replace("Â", "{Var85 161}").Replace("Ã", "{Var85 162}").Replace("Ä", "{Var85 163}")
                .Replace("—", "{Var85 87}").Replace("È", "{Var85 167}").Replace("É", "{Var85 168}").Replace("Ê", "{Var85 169}").Replace("Ì", "{Var85 171}")
                .Replace("Í", "{Var85 172}").Replace("Ò", "{Var85 177}").Replace("Ó", "{Var85 178}").Replace("Ô", "{Var85 179}").Replace("Õ", "{Var85 180}")
                .Replace("Ù", "{Var85 184}").Replace("Ú", "{Var85 185}").Replace("Ç", "{Var85 166}");

            //###### Salva o txt com os valores substituídos onde o usuário escolher ######//
            var s = Path.ChangeExtension(fi.FullName, ".string");
            File.WriteAllText(s, novoConteudoTxt, Encoding.UTF8);
            var fileInfo = new FileInfo(s);
            var fileStream = new FileStream(s, FileMode.Open, FileAccess.ReadWrite);
            Pack(fileStream, fileInfo);
            File.Delete(fi.FullName);
            File.Delete(s);
            var w = new WpdUnpack();
            w.repackWPDGlobalAlignment(fileInfo.DirectoryName, (uint)numericUpDown4.Value, (uint)numericUpDown3.Value);
        }
    }
}
