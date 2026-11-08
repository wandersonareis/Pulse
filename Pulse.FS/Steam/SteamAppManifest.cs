using System.IO;

namespace Pulse.FS
{
    public sealed class SteamAppManifest
    {
        public string AppId { get; }
        public string Name { get; }
        public string InstallDirectory { get; }

        private SteamAppManifest(string appId, string name, string installDirectory)
        {
            AppId = appId;
            Name = name;
            InstallDirectory = installDirectory;
        }

        public static SteamAppManifest Read(string path)
        {
            SteamKeyValues root = SteamKeyValues.Parse(File.ReadAllText(path));

            SteamKeyValues state = root.Child("AppState");
            if (state == null)
                state = root;

            return new(
                state.GetValue("appid"),
                state.GetValue("name"),
                state.GetValue("installdir"));
        }
    }
}
