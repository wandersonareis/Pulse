using System;
using System.IO;
using System.Linq;
using Pulse.FS;
using Xunit;

namespace Pulse.Tests
{
    public sealed class SteamTests
    {
        private const string LibraryFoldersLegacy = "\"libraryfolders\"\n{\n\t\"0\"\n\t{\n\t\t\"path\"\t\t\"D:\\\\Steam\"\n\t\t\"apps\"\n\t\t{\n\t\t\t\"345350\"\t\t\"22050349978\"\n\t\t}\n\t}\n\t\"1\"\n\t{\n\t\t\"path\"\t\t\"C:\\\\SteamLibrary\"\n\t}\n}\n";

        private const string LibraryFoldersModern = "\"libraryfolders\"\n{\n\t\"libraryfolders\"\n\t{\n\t\t\"0\"\n\t\t{\n\t\t\t\"path\"\t\t\"E:\\\\SteamLibrary\"\n\t\t\t\"label\"\t\t\"\"\n\t\t}\n\t}\n}\n";

        private const string AppManifest = "\"AppState\"\n{\n\t\"appid\"\t\t\"345350\"\n\t\"name\"\t\t\"LIGHTNING RETURNS: FINAL FANTASY XIII\"\n\t\"installdir\"\t\t\"LIGHTNING RETURNS FINAL FANTASY XIII\"\n\t\"InstalledDepots\"\n\t{\n\t\t\"345352\"\n\t\t{\n\t\t\t\"manifest\"\t\t\"1963913736343334081\"\n\t\t}\n\t}\n}\n";

        [Fact]
        public void KeyValues_ParsesNestedBlocks()
        {
            SteamKeyValues root = SteamKeyValues.Parse(LibraryFoldersLegacy);

            Assert.Equal("libraryfolders", root.Child("libraryfolders").Name);
            Assert.Equal("D:\\Steam", root.Child("libraryfolders").Child("0").GetValue("path"));
            Assert.Equal("C:\\SteamLibrary", root.Child("libraryfolders").Child("1").GetValue("path"));
            Assert.Equal("22050349978", root.Child("libraryfolders").Child("0").Child("apps").Child("345350").Value);
        }

        [Fact]
        public void LibraryFolders_LegacyFormat_ReturnsPaths()
        {
            string[] paths = ReadLibraryFolders(LibraryFoldersLegacy);

            Assert.Equal(2, paths.Length);
            Assert.Equal(SteamPath.Normalize("D:\\Steam"), paths[0]);
            Assert.Equal(SteamPath.Normalize("C:\\SteamLibrary"), paths[1]);
        }

        [Fact]
        public void LibraryFolders_ModernFormat_ReturnsPaths()
        {
            string[] paths = ReadLibraryFolders(LibraryFoldersModern);

            Assert.Single(paths);
            Assert.Equal(SteamPath.Normalize("E:\\SteamLibrary"), paths[0]);
        }

        [Fact]
        public void AppManifest_ReadsInstallDirectory()
        {
            string path = WriteTemp("appmanifest_345350.acf", AppManifest);

            try
            {
                SteamAppManifest manifest = SteamAppManifest.Read(path);

                Assert.Equal("345350", manifest.AppId);
                Assert.Equal("LIGHTNING RETURNS: FINAL FANTASY XIII", manifest.Name);
                Assert.Equal("LIGHTNING RETURNS FINAL FANTASY XIII", manifest.InstallDirectory);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void LibraryFolders_LocalInstallation_ReturnsLibraries()
        {
            const string path = @"D:\Steam\config\libraryfolders.vdf";

            if (!File.Exists(path))
                return;

            string[] paths = SteamLibraryFolders.Read(path);

            Assert.NotEmpty(paths);
            Assert.Contains(paths, p => p.EndsWith("Steam", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void AppManifest_LocalInstallation_ResolvesGameRoot()
        {
            const string manifestPath = @"D:\Steam\steamapps\appmanifest_345350.acf";

            if (!File.Exists(manifestPath))
                return;

            SteamAppManifest manifest = SteamAppManifest.Read(manifestPath);

            Assert.Equal("345350", manifest.AppId);
            Assert.Equal("LIGHTNING RETURNS FINAL FANTASY XIII", manifest.InstallDirectory);

            string root = Path.Combine(Path.GetDirectoryName(manifestPath) ?? string.Empty, "common", manifest.InstallDirectory);

            Assert.True(Directory.Exists(Path.Combine(root, "weiss_data", "sys")));
        }

        private static string[] ReadLibraryFolders(string content)
        {
            string path = WriteTemp("libraryfolders.vdf", content);

            try
            {
                return SteamLibraryFolders.Read(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static string WriteTemp(string name, string content)
        {
            string path = Path.Combine(Path.GetTempPath(), "pulse-tests", Guid.NewGuid().ToString("N") + "-" + name);

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, content);
            return path;
        }
    }
}
