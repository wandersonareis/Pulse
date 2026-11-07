using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.FS
{
    public static class ZtrTextUnitScanner
    {
        public static readonly string[] LanguageCodes = {"us", "jp", "fr", "de", "gr", "it", "sp", "ch", "kr"};

        public const string FileListDirectoryName = "filelist";

        public static bool TryParseUnitFileName(string name, out string baseName, out string language)
        {
            baseName = null;
            language = null;

            if (string.IsNullOrEmpty(name))
                return false;

            string stem = name;
            int dot = stem.LastIndexOf('.');
            if (dot > 0)
                stem = stem.Substring(0, dot);

            int separator = stem.LastIndexOf('_');
            if (separator > 0)
            {
                string candidate = stem.Substring(separator + 1).ToLowerInvariant();
                if (LanguageCodes.Contains(candidate))
                {
                    baseName = stem.Substring(0, separator);
                    language = candidate;
                    return true;
                }
            }

            baseName = stem;
            language = string.Empty;
            return true;
        }

        public static bool IsUnitLanguage(string code)
        {
            return code != null && LanguageCodes.Contains(code);
        }

        public static int GetLanguageRank(string code)
        {
            int index = Array.IndexOf(LanguageCodes, code);
            return index < 0 ? LanguageCodes.Length : index;
        }

        public static string BuildUnitId(string directory, string baseName)
        {
            if (string.IsNullOrEmpty(directory))
                return baseName;

            return directory + "/" + baseName;
        }

        public static bool TryGetContainerType(string path, out ZtrContainerType type)
        {
            type = ZtrContainerType.Loose;

            if (string.IsNullOrEmpty(path))
                return false;

            string extension = GetExtension(path);
            if (extension.Equals(".ztr", StringComparison.OrdinalIgnoreCase))
            {
                type = ZtrContainerType.Loose;
                return true;
            }

            if (extension.Equals(".bin", StringComparison.OrdinalIgnoreCase))
            {
                type = ZtrContainerType.Wpd;
                return true;
            }

            return false;
        }

        public static ZtrTextUnit[] Scan(IEnumerable<ZtrTextUnitFile> files, Func<string, string> typeResolver = null)
        {
            if (files == null)
                return Array.Empty<ZtrTextUnit>();

            Dictionary<string, Dictionary<string, Dictionary<string, ZtrTextUnitFile>>> groups = new(StringComparer.OrdinalIgnoreCase);

            foreach (ZtrTextUnitFile file in files)
            {
                if (file?.Path == null)
                    continue;

                string path = file.Path.Replace('\\', '/').TrimStart('/');

                int separator = path.LastIndexOf('/');
                string directory = separator < 0 ? string.Empty : path.Substring(0, separator);
                string name = separator < 0 ? path : path.Substring(separator + 1);

                if (IsFileListPath(directory))
                    continue;

                if (!TryParseUnitFileName(name, out string baseName, out string language))
                    continue;

                if (!groups.TryGetValue(directory, out Dictionary<string, Dictionary<string, ZtrTextUnitFile>> bases))
                {
                    bases = new(StringComparer.OrdinalIgnoreCase);
                    groups.Add(directory, bases);
                }

                if (!bases.TryGetValue(baseName, out Dictionary<string, ZtrTextUnitFile> languages))
                {
                    languages = new(StringComparer.OrdinalIgnoreCase);
                    bases.Add(baseName, languages);
                }

                if (languages.TryGetValue(language, out ZtrTextUnitFile previous) && previous.Container == ZtrContainerType.Loose)
                    continue;

                languages[language] = file;
            }

            List<ZtrTextUnit> result = new();
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, ZtrTextUnitFile>>> directory in groups)
            {
                foreach (KeyValuePair<string, Dictionary<string, ZtrTextUnitFile>> @base in directory.Value)
                {
                    ZtrTextUnit unit = BuildUnit(directory.Key, @base.Key, @base.Value, typeResolver);
                    if (unit != null)
                        result.Add(unit);
                }
            }

            result.Sort((x, y) => string.Compare(x.Id, y.Id, StringComparison.OrdinalIgnoreCase));
            return result.ToArray();
        }

        private static ZtrTextUnit BuildUnit(string directory, string baseName, Dictionary<string, ZtrTextUnitFile> files, Func<string, string> typeResolver)
        {
            if (!files.TryGetValue("us", out ZtrTextUnitFile primary))
                return null;

            string id = BuildUnitId(directory, baseName);

            List<string> languages = new();
            Dictionary<string, string> paths = new(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, ZtrTextUnitFile> pair in files)
            {
                paths.Add(pair.Key, pair.Value.Path);
                if (!string.IsNullOrEmpty(pair.Key))
                    languages.Add(pair.Key);
            }

            languages.Sort((x, y) =>
            {
                int rank = GetLanguageRank(x).CompareTo(GetLanguageRank(y));
                return rank != 0 ? rank : string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            });

            string type = typeResolver?.Invoke(id) ?? string.Empty;

            return new(
                id,
                directory,
                baseName,
                baseName,
                type,
                languages,
                paths,
                primary.Path,
                primary.Container);
        }

        private static bool IsFileListPath(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                return false;

            return directory.Equals(FileListDirectoryName, StringComparison.OrdinalIgnoreCase) ||
                   directory.StartsWith(FileListDirectoryName + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetExtension(string path)
        {
            int slash = path.LastIndexOfAny(new[] {'/', '\\'});
            string name = slash < 0 ? path : path.Substring(slash + 1);

            int dot = name.LastIndexOf('.');
            return dot < 0 ? string.Empty : name.Substring(dot);
        }
    }
}
