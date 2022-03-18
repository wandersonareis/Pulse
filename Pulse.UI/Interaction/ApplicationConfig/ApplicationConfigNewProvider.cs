using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigNewProvider : IInfoProvider<ApplicationConfigInfo>
    {
        public ApplicationConfigInfo Provide()
        {
            return new ApplicationConfigInfo();
        }

        public string Title => Lang.InfoProvider.ApplicationConfig.NewTitle;

        public string Description => Lang.InfoProvider.ApplicationConfig.NewDescription;
    }
}