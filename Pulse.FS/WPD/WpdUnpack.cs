using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pulse.FS
{
    public sealed class WpdUnpack
    {
        private readonly Utils _utils = new();

        private readonly byte[] _info4 = new byte[4];

        private readonly byte[] _wpdEntryNameBuffer = new byte[16];
        public byte[] ArrayReverse(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }
        public void UnpackWpd(FileStream fs, ref FileInfo fi)
        {
            string text = fi.DirectoryName + "\\_unpacked\\" + fi.Name;
            string cfgpath = fi.DirectoryName + "\\_cfg";
            string path2 = fi.DirectoryName + "\\_cfg\\" + fi.Name + ".txt";
            Directory.CreateDirectory(text);
            Directory.CreateDirectory(cfgpath);
            fs.Read(_info4, 0, 4);
            if (Encoding.ASCII.GetString(_info4) != "WPD\0")
            {
                MessageBox.Show("Invalid WPD/.wdb file!");
            }
            else
            {
                fs.Read(_info4, 0, 4);
                uint num = BitConverter.ToUInt32(ArrayReverse(_info4), 0);
                fs.Seek(8L, SeekOrigin.Current);
                List<string> fileNames = new();
                List<uint> list2 = new();
                List<uint> list3 = new();
                List<string> list4 = new();
                StreamWriter streamWriter = new(path2);
                for (int i = 0; i < num; i++)
                {
                    fs.Read(_wpdEntryNameBuffer, 0, 16);
                    streamWriter.WriteLine(i.ToString("d8") + "," + Encoding.UTF8.GetString(_wpdEntryNameBuffer).Replace("\0", string.Empty));
                    fileNames.Add(Encoding.UTF8.GetString(_wpdEntryNameBuffer).Replace(":", "[0x3A]_").Replace("\0", string.Empty));
                    fs.Read(_info4, 0, 4);
                    list2.Add(BitConverter.ToUInt32(ArrayReverse(_info4), 0));
                    fs.Read(_info4, 0, 4);
                    list3.Add(BitConverter.ToUInt32(ArrayReverse(_info4), 0));
                    fs.Read(_info4, 0, 4);
                    if (BitConverter.ToUInt32(_info4, 0) != 0)
                    {
                        list4.Add(Encoding.ASCII.GetString(_info4).Replace("\0", string.Empty));
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
                    string empty;
                    empty = list4[j] == string.Empty ? text + "\\" + j.ToString("d8") + "_" + fileNames[j] : text + "\\" + j.ToString("d8") + "_" + fileNames[j] + "." + list4[j];
                    File.WriteAllBytes(empty, array);
                    if (array.Length < 4) continue;
                    byte[] array2 = new byte[4];
                    Array.Copy(array, 0, array2, 0, 4);
                    if (BitConverter.ToString(array2).Replace("-", string.Empty) == "54545454")
                    {
                        _utils.RunFfxiiiCrypt("-d \"" + empty + "\" S");
                    }
                }
            }
        }

        public void RepackWpdGlobalAlignment(string selectedPath, uint alignment, uint dataBaseAlignment)
        {
            string text = selectedPath + "\\_repacked";
            DirectoryInfo directoryInfo = new(selectedPath);
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
            List<string> list = new();
            int count = array.Length;
            for (int j = count - 1; j >= 0; j--)
            {
                string[] array2 = array[j].Split(',');
                list.Add(array2[1]);
            }
            FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);
            fileStream.Write(Encoding.ASCII.GetBytes("WPD\0"), 0, 4);
            fileStream.Write(ArrayReverse(BitConverter.GetBytes(list.Count())), 0, 4);
            fileStream.Write(ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
            fileStream.Write(ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
            int num = 0;
            int k;
            for (k = list.Count * 32 + 16; k % dataBaseAlignment != 0; k++)
            {
            }
            num += k;
            string[] array3 = (from f in Directory.GetFiles(selectedPath)
                               orderby f
                               select f).ToArray();
            for (int l = 0; l < list.Count(); l++)
            {
                FileInfo fileInfo = new(array3[l]);
                byte[] bytes = Encoding.UTF8.GetBytes(list[l]);
                fileStream.Write(bytes, 0, bytes.Length);
                while (fileStream.Position % 16 != 0)
                {
                    fileStream.Write(new byte[1], 0, 1);
                }
                fileStream.Write(ArrayReverse(BitConverter.GetBytes(num)), 0, 4);
                fileStream.Write(ArrayReverse(BitConverter.GetBytes((int)fileInfo.Length)), 0, 4);
                for ( num += (int)fileInfo.Length; (long)num % (long)alignment != 0; num++)
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
                    fileStream.Write(ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
                }
                fileStream.Write(ArrayReverse(BitConverter.GetBytes(0)), 0, 4);
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
                        _utils.RunFfxiiiCrypt("-e \"" + selectedPath + "\\" + array3[m] + "\" S");
                        array4 = File.ReadAllBytes(selectedPath + "\\" + array3[m]);
                    }
                }
                fileStream.Write(array4, 0, array4.Length);
                while (fileStream.Position % alignment != 0)
                {
                    fileStream.Write(new byte[1], 0, 1);
                }
            }
            fileStream.Close();
        }
    }
}
