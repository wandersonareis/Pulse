using System.Collections.Generic;

namespace Pulse.FS
{
    public sealed class ZtrTextUnit
    {
        public string Id { get; }
        public string Directory { get; }
        public string Base { get; }
        public string Label { get; }
        public string Type { get; }
        public IReadOnlyList<string> Languages { get; }
        public IReadOnlyDictionary<string, string> Files { get; }
        public string Primary { get; }
        public ZtrContainerType PrimaryContainer { get; }

        public ZtrTextUnit(
            string id,
            string directory,
            string @base,
            string label,
            string type,
            IReadOnlyList<string> languages,
            IReadOnlyDictionary<string, string> files,
            string primary,
            ZtrContainerType primaryContainer)
        {
            Id = id;
            Directory = directory;
            Base = @base;
            Label = label;
            Type = type;
            Languages = languages;
            Files = files;
            Primary = primary;
            PrimaryContainer = primaryContainer;
        }

        public ZtrContainerType ContainerOf(string language)
        {
            return Files != null && Files.TryGetValue(language ?? string.Empty, out string path)
                ? ZtrTextUnitScanner.ResolveContainerType(path)
                : PrimaryContainer;
        }

        public ZtrTextUnitFile GetFile(string language)
        {
            if (Files == null || !Files.TryGetValue(language ?? string.Empty, out string path))
                return null;

            return ZtrTextUnitScanner.ProvideTextUnitFile(path);
        }

        public ZtrTextUnitFile GetPrimaryFile()
        {
            return Primary == null ? null : ZtrTextUnitScanner.ProvideTextUnitFile(Primary);
        }

        public string GetInnerPath(string language)
        {
            ZtrTextUnitFile file = GetFile(language);
            return file?.InnerPath;
        }

        public string GetInnerName(string language)
        {
            ZtrTextUnitFile file = GetFile(language);
            return file?.ProvideInnerName();
        }

        public bool IsLoose(string language)
        {
            ZtrTextUnitFile file = GetFile(language);
            return file != null && file.IsLoose;
        }

        public bool IsInner(string language)
        {
            ZtrTextUnitFile file = GetFile(language);
            return file != null && file.HasOuterZtr;
        }

        public bool HasInnerZtr(string language)
        {
            ZtrTextUnitFile file = GetFile(language);
            return file != null && file.HasInnerZtr;
        }
    }
}
