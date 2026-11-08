using System;
using System.Collections.Generic;
using System.IO;
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

        public static ZtrTextUnitFile ProvideTextUnitFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!TryGetContainerType(path, out ZtrContainerType type))
                return null;

            return type == ZtrContainerType.Loose
                ? ZtrTextUnitFile.ProvideLoose(path)
                : ZtrTextUnitFile.ProvideContainer(path, null);
        }

        public static ZtrContainerType ResolveContainerType(string path)
        {
            return TryGetContainerType(path, out ZtrContainerType type) ? type : ZtrContainerType.Loose;
        }

        public static string ResolveInnerZtrName(byte[] containerBytes, string containerPath)
        {
            if (containerBytes == null || containerBytes.Length < ZtrContainer.HeaderSize)
                return null;

            using (MemoryStream input = new(containerBytes))
                return ResolveInnerZtrName(input, containerPath);
        }

        public static string ResolveInnerZtrName(Stream container, string containerPath)
        {
            if (container == null || !container.CanSeek)
                return null;

            long position = container.Position;

            try
            {
                container.Position = 0;

                if (!TryReadWpdHeader(container, out WpdHeader header))
                    return null;

                WpdEntry entry = ZtrContainer.FindZtrEntry(header);
                if (entry == null)
                    return null;

                return entry.NameWithoutExtension + ".ztr";
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                container.Position = position;
            }
        }

        private static bool TryReadWpdHeader(Stream input, out WpdHeader header)
        {
            header = null;

            byte[] magic = new byte[4];
            if (ReadExact(input, magic, 4) != 4)
                return false;

            if (magic[0] != 0x57 || magic[1] != 0x50 || magic[2] != 0x44 || magic[3] != 0x00)
                return false;

            byte[] head = new byte[12];
            if (ReadExact(input, head, 12) != 12)
                return false;

            int count = (head[0] << 24) | (head[1] << 16) | (head[2] << 8) | head[3];
            if (count < 0 || count > 0x10000)
                return false;

            input.Position = ZtrContainer.HeaderSize;

            byte[] table = new byte[count * ZtrContainer.RecordSize];
            if (count > 0 && ReadExact(input, table, table.Length) != table.Length)
                return false;

            header = new WpdHeader
            {
                Magic = WpdHeader.MagicNumber,
                Count = count,
                Entries = new WpdEntry[count]
            };

            for (int i = 0; i < count; i++)
            {
                int offset = i * ZtrContainer.RecordSize;

                int nameLength = 0;
                while (nameLength < 16 && table[offset + nameLength] != 0)
                    nameLength++;

                string name = System.Text.Encoding.ASCII.GetString(table, offset, nameLength);
                int value = ReadBigInt32(table, offset + 16);
                int length = ReadBigInt32(table, offset + 20);

                int extensionLength = 0;
                while (extensionLength < 8 && table[offset + 24 + extensionLength] != 0)
                    extensionLength++;

                string extension = System.Text.Encoding.ASCII.GetString(table, offset + 24, extensionLength);

                header.Entries[i] = new(i, name, value, length, extension);
            }

            return true;
        }

        private static int ReadBigInt32(byte[] data, int offset)
        {
            return (int)((uint)(data[offset] << 24) | (uint)(data[offset + 1] << 16) | (uint)(data[offset + 2] << 8) | data[offset + 3]);
        }

        private static int ReadExact(Stream input, byte[] buffer, int size)
        {
            int total = 0;
            while (total < size)
            {
                int readed = input.Read(buffer, total, size - total);
                if (readed <= 0)
                    break;

                total += readed;
            }

            return total;
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
