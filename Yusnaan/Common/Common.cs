using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yusnaan.Common
{
    internal static class Commons
    {
        internal static string? GetFolder(string title = "")
        {
            return Dialogs.GetFolder(title);
        }
        internal static string? GetStringsFile(string fileName)
        {
            return Dialogs.GetFile("Get strings file!",
                $"Dialogs file|{Path.GetFileNameWithoutExtension(fileName)}.strings");
        }

        internal static string? GetScenarioFile(string? oldPath = "")
        {
            return Dialogs.GetFile("Get Scenario File", "Scenario file|*.bin", oldPath);
        }
        internal static string[]? GetScenarioFiles(string? oldPath = "")
        {
            return Dialogs.GetFiles("Get Scenario Files", "Scenario file|*.bin", oldPath);
        }
        public static string? CheckStringsFile(IEnumerable<string> file)
        {
            return file.Select(s => File.Exists(s) ? s : GetStringsFile(s)).FirstOrDefault();
        }
        public static string? CheckStringsFile(string file)
        {
            return File.Exists(file) ? file : GetStringsFile(file);
        }
    }
}
