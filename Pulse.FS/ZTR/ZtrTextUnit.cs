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
    }
}
