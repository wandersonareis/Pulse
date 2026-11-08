using Pulse.Core;

namespace Pulse.UI
{
    public sealed class GameLocationProviders : InfoProviderGroup<GameLocationInfo>
    {
        public GameLocationProviders()
            : base(Lang.InfoProvider.GameLocation.Title, Lang.InfoProvider.GameLocation.Description)
        {
            Capacity = 4;
            Add(new GameLocationConfigurationProvider());
            Add(new GameLocationSteamLibraryProvider());
            Add(new GameLocationSteamRegistryProvider());
            Add(new GameLocationUserProvider());
        }
    }
}