using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class GameLocationSteamLibraryProvider : IInfoProvider<GameLocationInfo>
    {
        public const int SteamAppIdPart1 = 292120;
        public const int SteamAppIdPart2 = 292140;
        public const int SteamAppIdPart3 = 345350;

        public const string SteamRegistryPath = @"Software\Valve\Steam";
        public const string SteamRegistryMachinePath = @"SOFTWARE\Valve\Steam";
        public const string SteamPathValueName = "SteamPath";
        public const string SteamExeValueName = "SteamExe";
        public const string SteamInstallPathValueName = "InstallPath";

        public GameLocationInfo Provide()
        {
            string steamDirectory = LocateSteamDirectory();
            string appId = SteamAppId;

            foreach (string library in EnumerateLibraries(steamDirectory))
            {
                string manifestPath = Path.Combine(library, "steamapps", "appmanifest_" + appId + ".acf");
                if (!File.Exists(manifestPath))
                    continue;

                SteamAppManifest manifest = SteamAppManifest.Read(manifestPath);
                if (string.IsNullOrEmpty(manifest.InstallDirectory))
                    continue;

                string root = Path.Combine(library, "steamapps", "common", manifest.InstallDirectory);

                GameLocationInfo result = new(root);
                result.Validate();

                return result;
            }

            throw Exceptions.CreateException("Não foi encontrado o jogo nas bibliotecas do Steam.");
        }

        public string Title => Lang.InfoProvider.GameLocation.SteamLibraryTitle;

        public string Description => Lang.InfoProvider.GameLocation.SteamLibraryDescription;

        private static string SteamAppId
        {
            get
            {
                switch (InteractionService.GamePart)
                {
                    case FFXIIIGamePart.Part1:
                        return SteamAppIdPart1.ToString();
                    case FFXIIIGamePart.Part2:
                        return SteamAppIdPart2.ToString();
                    case FFXIIIGamePart.Part3:
                        return SteamAppIdPart3.ToString();
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static IEnumerable<string> EnumerateLibraries(string steamDirectory)
        {
            string[] paths = SteamLibraryFolders.Read(Path.Combine(steamDirectory, "config", "libraryfolders.vdf"));

            if (paths.Length == 0)
                paths = SteamLibraryFolders.Read(Path.Combine(steamDirectory, "steamapps", "libraryfolders.vdf"));

            foreach (string path in paths)
                yield return path;

            if (paths.Length == 0)
                yield return steamDirectory;
        }

        private static string LocateSteamDirectory()
        {
            foreach (RegistryView view in new[] {RegistryView.Registry64, RegistryView.Registry32})
            {
                string result = ReadValue(RegistryHive.CurrentUser, view, SteamRegistryPath, SteamPathValueName);
                if (result != null)
                    return result;

                result = ReadValue(RegistryHive.CurrentUser, view, SteamRegistryPath, SteamExeValueName);
                if (result != null)
                {
                    string directory = Path.GetDirectoryName(result);
                    if (directory != null)
                    {
                        string normalized = SteamPath.Normalize(directory);
                        if (normalized != null)
                            return normalized;
                    }
                }

                result = ReadValue(RegistryHive.LocalMachine, view, SteamRegistryMachinePath, SteamInstallPathValueName);
                if (result != null)
                    return result;
            }

            throw Exceptions.CreateException("Não foi encontrada nenhuma entrada do Steam no registro.");
        }

        private static string ReadValue(RegistryHive hive, RegistryView view, string keyPath, string valueName)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
                using (RegistryKey key = baseKey.OpenSubKey(keyPath))
                {
                    if (key == null)
                        return null;

                    if (key.GetValue(valueName) is not string value)
                        return null;

                    return SteamPath.Normalize(value);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
