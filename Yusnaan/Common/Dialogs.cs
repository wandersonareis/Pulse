using System.Linq;
using System.Windows;
using Meziantou.Framework;
using Ookii.Dialogs.Wpf;

namespace Yusnaan.Common;

internal class Dialogs
{
    private const string ScenarioFilters =
        "Scenarie US|*_us.bin;|Scenarie SP|*_sp.bin;|Scenarie FR|*_fr.bin;|Scenarie files|*_ch.bin;*_fr.bin;*_gr.bin;*_it.bin;*_jp.bin;*_kr.bin;*_sp.bin;*_us.bin;";
    public static string? ShowFolderBrowserDialog(string description)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = description,
            UseDescriptionForTitle = true // This applies to the Vista style dialog only, not the old dialog.
        };
        bool? folder = dialog.ShowDialog();
        return folder == VistaFileDialog.IsVistaFileDialogSupported ? dialog.SelectedPath : null;
    }

    internal static FullPath? GetScenarioFile(string? oldPath = "")
    {
        return GetFile("Get Scenarie File", ScenarioFilters, oldPath);
    }
    internal static FileInfo? TestGetScenarioFile(string? oldPath = "")
    {
        return GetFileAsync("Get Scenarie File", ScenarioFilters, oldPath);
    }

    internal static string[]? GetScenarioFiles(string? oldPath = "")
    {
        return GetFiles("Get Scenarie Files", ScenarioFilters, oldPath);
    }
    internal static FileInfo[]? GetScenarioFilesAsync(string? oldPath = "")
    {
        return GetFilesAsync("Get Scenarie Files", ScenarioFilters, oldPath);
    }
    internal static FileInfo[]? GetStringsFilesAsync()
    {
        return GetFilesAsync("Get Strings Files");
    }
    public static string? CheckStringsFile(IEnumerable<string> file)
    {
        return file.Select(s => File.Exists(s) ? s : GetStringsFile(s)).FirstOrDefault();
    }
    public static string? CheckStringsFile(string file)
    {
        return File.Exists(file) ? file : GetStringsFile(file);
    }

    internal static string? GetStringsFile(string fileName)
    {
        return GetFile("Get strings file!",
            $"Dialogs file|{Path.GetFileNameWithoutExtension(fileName)}.strings");
    }

    public static FullPath? GetFile(string title, string filters = "", string? initial = "")
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = title,
            InitialDirectory = initial,
            Filter = filters,
            CheckFileExists = true
        };

        bool? folder = dialog.ShowDialog();
        return folder == VistaFileDialog.IsVistaFileDialogSupported ? FullPath.FromPath(dialog.FileName) : null;
    }

    private static FileInfo? GetFileAsync(string title, string filters = "", string? initial = "")
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = title,
            InitialDirectory = initial,
            Filter = filters,
            CheckFileExists = true
        };

        bool? folder = dialog.ShowDialog();
        return folder == VistaFileDialog.IsVistaFileDialogSupported ? new FileInfo(dialog.FileName) : null;
    }

    private static string[]? GetFiles(string title, string filters = "", string? initial = "")
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = title,
            InitialDirectory = initial,
            Filter = string.IsNullOrEmpty(filters) ? "Strings files (*.strings)|*.strings" : filters,
            RestoreDirectory = true,
            Multiselect = true,
        };

        bool? folder = dialog.ShowDialog();
        return folder == VistaFileDialog.IsVistaFileDialogSupported ? dialog.FileNames : null;
    }

    private static FileInfo[]? GetFilesAsync(string title, string filters = "", string? initial = "")
    {
        VistaOpenFileDialog dialog = new()
        {
            Title = title,
            InitialDirectory = initial,
            Filter = string.IsNullOrEmpty(filters) ? "Strings files (*.strings)|*.strings" : filters,
            RestoreDirectory = true,
            Multiselect = true
        };

        bool? folder = dialog.ShowDialog();
        return folder != VistaFileDialog.IsVistaFileDialogSupported ? null : dialog.FileNames.Select(x => new FileInfo(x)).ToArray();
    }
    public static string? GetZtrFile(string name, string? file)
    {
        if (File.Exists(Path.GetFileName(name)))
        {
            return Path.GetFileName(name);
        }

        return File.Exists(name) ? name : GetNewZtrFile(file);
    }

    public static string? GetNewZtrFile(string? file)
    {
        string? ztr = Path.ChangeExtension(file, ".ztr");

        return File.Exists(ztr) ? ztr : GetFile($"Get {Path.GetFileName(ztr)} File", $"Ztr File|{Path.GetFileName(ztr)}");
    }
}