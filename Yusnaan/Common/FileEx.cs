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

    public static FileStreamOptions FileStreamOutputOptions()
    {
        return new FileStreamOptions {Mode = FileMode.Create, Access = FileAccess.ReadWrite, Share = FileShare.Read, Options = FileOptions.Asynchronous, BufferSize = 2048};
    }
    public static FileStreamOptions FileStreamInputOptions()
    {
        return new FileStreamOptions {Mode = FileMode.Open, Access = FileAccess.ReadWrite, Share = FileShare.Read, Options = FileOptions.Asynchronous, BufferSize = 2048};
    }
}