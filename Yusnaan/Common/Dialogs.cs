using System.IO;
using System.Windows.Forms;

namespace Yusnaan.Common;

internal class Dialogs
{
    public static string? GetFolder(string title)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = title
        };
        var folder = dialog.ShowDialog();
        return folder == DialogResult.OK ? dialog.SelectedPath : null;
    }
    public static string? GetFile(string title, string filters = "", string? initial = "")
    {
        using OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = initial ?? string.Empty,
            Filter = string.IsNullOrEmpty(filters) ? "Strings files (*.strings)|*.strings" : filters,
            RestoreDirectory = true,
            Multiselect = false,
            Title = title
        };

        if (openFileDialog.ShowDialog() == DialogResult.Cancel) return null;
        //Get the path of specified file
        string filePath = openFileDialog.FileName;

        return filePath;
    }
    public static string[]? GetFiles(string title, string filters = "", string? initial = "")
    {
        using OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = initial ?? string.Empty,
            Filter = string.IsNullOrEmpty(filters) ? "Strings files (*.strings)|*.strings" : filters,
            RestoreDirectory = true,
            Multiselect = true,
            Title = title
        };

        if (openFileDialog.ShowDialog() == DialogResult.Cancel) return null;
        //Get the path of specified file
        string[] filePath = openFileDialog.FileNames;

        return filePath;
    }
    public static string? GetZtrFile(string name, string? file)
    {
        return File.Exists(name) ? name : GetNewZtrFile(file);
    }

    public static string? GetNewZtrFile(string? file)
    {
        string ztr = Path.ChangeExtension(file, ".ztr");

        return File.Exists(ztr) ? ztr : GetFile($"Get {Path.GetFileName(ztr)} File", $"Ztr File|{Path.GetFileName(ztr)}");
    }
}