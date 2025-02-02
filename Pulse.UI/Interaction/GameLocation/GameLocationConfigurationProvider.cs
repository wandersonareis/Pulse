﻿using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationConfigurationProvider : IInfoProvider<GameLocationInfo>
    {
        public GameLocationInfo Provide()
        {
            GameLocationInfo value = InteractionService.Configuration.Provide().GameLocation;
            value.Validate();
            return value;
        }

        public string Title => Lang.InfoProvider.GameLocation.ConfigurationTitle;

        public string Description => Lang.InfoProvider.GameLocation.ConfigurationDescription;
    }
}