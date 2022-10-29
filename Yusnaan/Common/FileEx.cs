using System.Linq;
using Meziantou.Framework;
using Pulse.Core;
using Pulse.FS;
using SimpleLogger;
using DifferenceEngine;
using Yusnaan.Model.Extractors;
using static SimpleLogger.Logger;
using System.Collections;
using System.Windows.Forms;

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
    
    public static bool ContentEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary) where TKey : notnull
    {
        return otherDictionary
            .OrderBy(kvp => kvp.Key)
            .SequenceEqual(dictionary
                .OrderBy(kvp => kvp.Key));
    }

    public static bool CheckDummyFile(this FileInfo fileInfo)
    {
        var header = fileInfo.OpenRead().ReadContent<WdbHeader>();
        WpdEntry entry = header.Entries[1];

        if (header.Entries.Length < 2) return false;

        if (string.Equals(entry.NameWithoutExtension, "dummy", StringComparison.InvariantCultureIgnoreCase) ||
            !string.Equals(entry.Extension, "ztr", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        Logger.Log<WpdZtrUnpack>(Logger.Level.Error, $"This {fileInfo.Name} contains dummy ztr or not contains ztr file!");

        return false;

    }
    public static void Check(this string original, string newFile)
    {

        ZtrFileEntry[] books, reviews;
        using Stream book = File.OpenRead(original);
        using Stream review = File.OpenRead(newFile);
        string name;
        ZtrTextReader reader = new(book, StringsZtrFormatter.Instance);
        books = reader.Read(out name);
        reader = new ZtrTextReader(review, StringsZtrFormatter.Instance);
        reviews = reader.Read(out name);

        if (!books.ToDictionary(e => e.Key, e => e.Value)
                .ContentEquals(reviews.ToDictionary(e => e.Key, e => e.Value)))
        {
            TextDiff(original, newFile);
            using StreamWriter w = File.AppendText("myFile.txt");
            w.WriteLine(Path.GetFileNameWithoutExtension(original));
            //new TaskDialogs().ShowWarningDialog("Arquivos não são iguais!", "", $"{original}");
        }
    }
    private static void TextDiff(string sFile, string dFile)
    {
        DiffListTextFile sLf;
        DiffListTextFile dLf;
        try
        {
            sLf = new DiffListTextFile(sFile);
            dLf = new DiffListTextFile(dFile);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, @"File Error");
            return;
        }

        try
        {
            double time = 0;
            DiffEngine de = new DiffEngine();
            time = de.ProcessDiff(sLf, dLf, DiffEngineLevel.FastImperfect);

            ArrayList rep = de.DiffReport();
            Results dlg = new(sLf, dLf, rep, time);
            dlg.ShowDialog();
            dlg.Dispose();
        }
        catch (Exception ex)
        {
            string tmp = string.Format("{0}{1}{1}***STACK***{1}{2}",
                ex.Message,
                Environment.NewLine,
                ex.StackTrace);
            MessageBox.Show(tmp, @"Compare Error");
        }
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