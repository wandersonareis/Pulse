using System;

namespace Pulse.FS
{
    public sealed class ZtrTextUnitFile
    {
        public string Path { get; }
        public ZtrContainerType Container { get; }
        public string InnerPath { get; }

        public bool IsLoose => Container == ZtrContainerType.Loose;
        public bool HasInnerZtr => InnerPath != null;
        public bool HasOuterZtr => InnerPath != null && InnerPath != Path;

        private ZtrTextUnitFile(string path, ZtrContainerType container, string innerPath)
        {
            Path = path;
            Container = container;
            InnerPath = innerPath;
        }

        public static ZtrTextUnitFile ProvideLoose(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return new(path, ZtrContainerType.Loose, path);
        }

        public static ZtrTextUnitFile ProvideContainer(string path, string innerPath)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            string result = string.IsNullOrEmpty(innerPath) ? null : innerPath;

            return new(path, ZtrContainerType.Wpd, result);
        }

        public static ZtrTextUnitFile Provide(string path, ZtrContainerType container)
        {
            return container == ZtrContainerType.Loose ? ProvideLoose(path) : ProvideContainer(path, null);
        }

        public string ProvideInnerName()
        {
            if (InnerPath == null)
                return Path;

            int index = InnerPath.LastIndexOfAny(new[] {'/', '\\'});
            return index < 0 ? InnerPath : InnerPath.Substring(index + 1);
        }
    }
}
