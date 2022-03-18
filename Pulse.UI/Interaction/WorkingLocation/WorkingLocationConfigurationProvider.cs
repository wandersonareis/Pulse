using Pulse.Core;

namespace Pulse.UI
{
    public sealed class WorkingLocationConfigurationProvider : IInfoProvider<WorkingLocationInfo>
    {
        public WorkingLocationInfo Provide()
        {
            WorkingLocationInfo value = InteractionService.Configuration.Provide().WorkingLocation;
            value.Validate();
            return value;
        }

        public string Title => Lang.InfoProvider.WorkingLocation.ConfigurationTitle;

        public string Description => Lang.InfoProvider.WorkingLocation.ConfigurationDescription;
    }
}