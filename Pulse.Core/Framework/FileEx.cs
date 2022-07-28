using System;
using System.IO;
using System.Linq;

namespace Pulse.Core
{
    public static class FileEx
    {
        public static long GetSize(string path)
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return fileStream.Length;
        }

        public static long GetSize(FileSystemInfo fsi)
        {
            Exceptions.CheckArgumentNull(fsi, "fsi");

            try
            {
                switch (fsi)
                {
                    case FileInfo fileInfo:
                        return fileInfo.Length;
                    case DirectoryInfo directoryInfo:
                        return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
                    default:
                        Log.Warning("[FileEx]Неизвестный наследник FileSystemInfo: {0}", fsi.GetType());
                        return 0;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[FileEx]Непредвиденная ошибка.");
                return 0;
            }
        }
    }
}