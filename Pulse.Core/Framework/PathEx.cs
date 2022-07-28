using System.IO;

namespace Pulse.Core
{
    public static class PathEx
    {
        public static string ChangeName(string filePath, string newName)
        {
            string directory = Path.GetDirectoryName(filePath);
            return string.IsNullOrEmpty(directory) ? newName : Path.Combine(directory, newName);
        }

        public static string GetMultiDotComparableExtension(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            int index = fileName.LastIndexOf('.');
            switch (index)
            {
                case < 0:
                    return string.Empty;
                case > 0:
                {
                    int secondIndex = fileName.LastIndexOf('.', index - 1);
                    if (secondIndex > 0)
                        return fileName.Substring(secondIndex).ToLower();
                    break;
                }
            }

            return fileName.Substring(index).ToLower();
        }

        public static string ChangeMultiDotExtension(string filePath, string targetExtension)
        {
            string extension = GetMultiDotComparableExtension(filePath);
            if (extension != string.Empty)
                filePath = filePath.Substring(0, filePath.Length - extension.Length);

            if (string.IsNullOrEmpty(targetExtension))
                return filePath;

            if (targetExtension[0] != '.')
                targetExtension = '.' + targetExtension;

            return filePath + targetExtension;
        }
    }
}