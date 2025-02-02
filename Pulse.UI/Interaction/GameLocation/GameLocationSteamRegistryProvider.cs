﻿using System;
using Microsoft.Win32;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationSteamRegistryProvider : IInfoProvider<GameLocationInfo>
    {
        public const string SteamRegistyPart1Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120";
        public const string SteamRegistyPart2Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292140";
        public const string SteamRegistyPart3Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 345350";
        public const string SteamGamePathTag = @"InstallLocation";

        public GameLocationInfo Provide()
        {
            using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry32))
            using (RegistryKey registryKey = localMachine.OpenSubKey(SteamRegistyPath))
            {
                if (registryKey == null)
                    throw Exceptions.CreateException("Não foi encontrada nenhuma entrada no registro.");

                GameLocationInfo result = new((string)registryKey.GetValue(SteamGamePathTag));
                result.Validate();

                return result;
            }
        }

        private static string SteamRegistyPath
        {
            get
            {
                switch (InteractionService.GamePart)
                {
                    case FFXIIIGamePart.Part1:
                        return SteamRegistyPart1Path;
                    case FFXIIIGamePart.Part2:
                        return SteamRegistyPart2Path;
                    case FFXIIIGamePart.Part3:
                        return SteamRegistyPart3Path;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public string Title => Lang.InfoProvider.GameLocation.SteamRegistryTitle;

        public string Description => Lang.InfoProvider.GameLocation.SteamRegistryDescription;
    }
}