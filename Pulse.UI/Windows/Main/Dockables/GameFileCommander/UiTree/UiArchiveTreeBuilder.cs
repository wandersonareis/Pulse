using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiArchiveTreeBuilder
    {
        private readonly GameLocationInfo _gameLocation;

        public UiArchiveTreeBuilder(GameLocationInfo gameLocation)
        {
            _gameLocation = gameLocation;
        }

        public UiArchives Build()
        {
            string[] lists = _gameLocation.EnumerateListingFiless().ToArray();
            ConcurrentBag<UiArchiveNode> nodes = new();

            Parallel.ForEach(lists, fileName =>
            {
                ArchiveAccessor accessor = new(GetBinaryFilePath(fileName), fileName);
                nodes.Add(new(accessor, null));
            });

            return new(nodes.OrderBy(n=>n.Name).ToArray());
        }

        private static string GetBinaryFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? throw new ArgumentNullException("filePath");
            string fileName = Path.GetFileName(filePath);

            if (InteractionService.GamePart == FFXIIIGamePart.Part3)
                return GetPart3BinaryFilePath(directory, fileName);

            if (fileName.StartsWith("filelist_scr", StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_scr", "white_scr"));
            if (fileName.StartsWith("filelist_patch", StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_patch", "white_patch"));

            return Path.Combine(directory, fileName.Replace("filelist", "white_img"));
        }

        private static string GetPart3BinaryFilePath(string directory, string fileName)
        {
            string key = GetListingKey(fileName);
            if (key != null)
            {
                foreach (string candidate in Directory.EnumerateFiles(directory, "white*.bin"))
                {
                    if (string.Equals(GetWhiteKey(Path.GetFileName(candidate)), key, StringComparison.OrdinalIgnoreCase))
                        return candidate;
                }
            }

            return Path.Combine(directory, fileName.Replace("filelist", "white_img"));
        }

        private static string GetListingKey(string fileName)
        {
            const string prefix = "filelist";

            if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            int start = prefix.Length;
            int end = fileName.IndexOf('.', start);
            if (end < 0)
                end = fileName.Length;

            if (end <= start)
                return null;

            return fileName.Substring(start, end - start);
        }

        private static string GetWhiteKey(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            if (name.EndsWith(".win32", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".win32".Length);

            if (name.StartsWith("white", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("white".Length);

            if (name.EndsWith("_img2", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "_img2".Length);
            else if (name.EndsWith("_img", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "_img".Length);

            if (name.StartsWith("img", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("img".Length);

            return name;
        }
    }
}
