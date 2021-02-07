using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pulse.FS
{
    public sealed class WpdUnpack
    {
        public void unpackWpd() { }

        public void repackWPDGlobalAlignment() { }

        private Utils utils = new Utils();

        private byte[] info4 = new byte[4];

        private byte[] wpdEntryNameBuffer = new byte[16];
        public byte[] ArrayReverse(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }
        public void unpackWpd(FileStream fs, ref FileInfo fi)
        {
            string text2 = fi.DirectoryName + "\\_unpacked";
            string text = fi.DirectoryName + "\\_unpacked\\" + fi.Name;
            string path = fi.DirectoryName + "\\_cfg";
            string path2 = fi.DirectoryName + "\\_cfg\\" + fi.Name + ".txt";
            Directory.CreateDirectory(text);
            Directory.CreateDirectory(path);
            fs.Read(this.info4, 0, 4);
            if (Encoding.ASCII.GetString(this.info4) != "WPD\0")
            {
                MessageBox.Show("Invalid WPD/.wdb file!");
            }
            else
            {
                fs.Read(this.info4, 0, 4);
                uint num = BitConverter.ToUInt32(this.ArrayReverse(this.info4), 0);
                fs.Seek(8L, SeekOrigin.Current);
                List<string> list = new List<string>();
                List<uint> list2 = new List<uint>();
                List<uint> list3 = new List<uint>();
                List<string> list4 = new List<string>();
                StreamWriter streamWriter = new StreamWriter(path2);
                for (int i = 0; i < num; i++)
                {
                    fs.Read(this.wpdEntryNameBuffer, 0, 16);
                    streamWriter.WriteLine(i.ToString("d8") + "," + Encoding.UTF8.GetString(this.wpdEntryNameBuffer).Replace("\0", string.Empty));
                    list.Add(Encoding.UTF8.GetString(this.wpdEntryNameBuffer).Replace(":", "[0x3A]_").Replace("\0", string.Empty));
                    fs.Read(this.info4, 0, 4);
                    list2.Add(BitConverter.ToUInt32(this.ArrayReverse(this.info4), 0));
                    fs.Read(this.info4, 0, 4);
                    list3.Add(BitConverter.ToUInt32(this.ArrayReverse(this.info4), 0));
                    fs.Read(this.info4, 0, 4);
                    if (BitConverter.ToUInt32(this.info4, 0) != 0)
                    {
                        list4.Add(Encoding.ASCII.GetString(this.info4).Replace("\0", string.Empty));
                    }
                    else
                    {
                        list4.Add(string.Empty);
                    }
                    fs.Seek(4L, SeekOrigin.Current);
                }
                streamWriter.Close();
                for (int j = 0; j < num; j++)
                {
                    fs.Seek(list2[j], SeekOrigin.Begin);
                    byte[] array = new byte[list3[j]];
                    fs.Read(array, 0, array.Length);
                    string empty = string.Empty;
                    empty = ((!(list4[j] != string.Empty)) ? (text + "\\" + j.ToString("d8") + "_" + list[j]) : (text + "\\" + j.ToString("d8") + "_" + list[j] + "." + list4[j]));
                    File.WriteAllBytes(empty, array);
                    if (array.Length >= 4)
                    {
                        byte[] array2 = new byte[4];
                        Array.Copy(array, 0, array2, 0, 4);
                        if (BitConverter.ToString(array2).Replace("-", string.Empty) == "54545454")
                        {
                            this.utils.runFfxiiiCrypt("-d \"" + empty + "\" S");
                        }
                    }
                }
            }
        }

        public void repackWPDGlobalAlignment(string selectedPath, uint alignment, uint dataBaseAlignment)
        {
            string text = selectedPath + "\\_repacked";
            DirectoryInfo directoryInfo = new DirectoryInfo(selectedPath);
            string path = text + "\\" + directoryInfo.Name;
            if (Directory.Exists(text))
            {
                string[] files = Directory.GetFiles(text);
                foreach (string path2 in files)
                {
                    File.Delete(path2);
                }
                Directory.Delete(text);
            }
            Directory.CreateDirectory(text);
            string[] array = File.ReadAllLines(selectedPath + "\\..\\..\\_cfg\\" + directoryInfo.Name + ".txt");
            List<string> list = new List<string>();
            for (int j = 0; j < array.Count(); j++)
            {
                string[] array2 = array[j].Split(',');
                list.Add(array2[1]);
            }
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            fileStream.Write(Encoding.ASCII.GetBytes("WPD\0"), 0, 4);
            fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(list.Count())), 0, 4);
            fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
            fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
            int num = 0;
            int k;
            for (k = list.Count() * 32 + 16; (long)k % (long)dataBaseAlignment != 0; k++)
            {
            }
            num += k;
            string[] array3 = (from f in Directory.GetFiles(selectedPath)
                               orderby f
                               select f).ToArray();
            for (int l = 0; l < list.Count(); l++)
            {
                FileInfo fileInfo = new FileInfo(array3[l]);
                byte[] bytes = Encoding.UTF8.GetBytes(list[l]);
                fileStream.Write(bytes, 0, bytes.Length);
                while (fileStream.Position % 16 != 0)
                {
                    fileStream.Write(new byte[1], 0, 1);
                }
                fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(num)), 0, 4);
                fileStream.Write(this.ArrayReverse(BitConverter.GetBytes((int)fileInfo.Length)), 0, 4);
                for (num += (int)fileInfo.Length; (long)num % (long)alignment != 0; num++)
                {
                }
                if (fileInfo.Extension != string.Empty)
                {
                    byte[] bytes2 = Encoding.ASCII.GetBytes(fileInfo.Extension.Replace(".", string.Empty));
                    fileStream.Write(bytes2, 0, bytes2.Length);
                    while (fileStream.Position % 4 != 0)
                    {
                        fileStream.Write(new byte[1], 0, 1);
                    }
                }
                else
                {
                    fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
                }
                fileStream.Write(this.ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
            }
            while (fileStream.Position % (long)dataBaseAlignment != 0)
            {
                fileStream.Write(new byte[1], 0, 1);
            }
            for (int m = 0; m < array3.Count(); m++)
            {
                byte[] array4 = File.ReadAllBytes(array3[m]);
                if (array4.Length >= 4)
                {
                    byte[] array5 = new byte[4];
                    Array.Copy(array4, 0, array5, 0, 4);
                    if (BitConverter.ToString(array5).Replace("-", string.Empty) == "54545454")
                    {
                        this.utils.runFfxiiiCrypt("-e \"" + selectedPath + "\\" + array3[m] + "\" S");
                        array4 = File.ReadAllBytes(selectedPath + "\\" + array3[m]);
                    }
                }
                fileStream.Write(array4, 0, array4.Length);
                while (fileStream.Position % (long)alignment != 0)
                {
                    fileStream.Write(new byte[1], 0, 1);
                }
            }
            fileStream.Close();
        }
    }
}
