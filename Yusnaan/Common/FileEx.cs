using Pulse.Core;

namespace Yusnaan.Common;

public static class FileEx
{
    public static string GetFileNameMultiDotExtension(this string filePath)
    {
        string extension = PathEx.GetMultiDotComparableExtension(filePath);
        if (extension != string.Empty)
            filePath = filePath[..^extension.Length];

        return filePath;
    }
}