namespace Pulse.FS
{
    public sealed class ZtrTextUnitFile
    {
        public string Path { get; }
        public ZtrContainerType Container { get; }

        public ZtrTextUnitFile(string path, ZtrContainerType container)
        {
            Path = path;
            Container = container;
        }
    }
}
