using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            if (fileName.StartsWith("filelist_scr", StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_scr", "white_scr"));
            if (fileName.StartsWith("filelist_patch", StringComparison.InvariantCultureIgnoreCase))
                return Path.Combine(directory, fileName.Replace("filelist_patch", "white_patch"));

            return Path.Combine(directory, fileName.Replace("filelist", "white_img"));
        }
    }
}