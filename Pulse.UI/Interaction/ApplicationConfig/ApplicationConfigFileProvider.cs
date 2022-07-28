using Pulse.Core;

namespace Pulse.UI.Interaction
{
    public sealed class ApplicationConfigFileProvider : IInfoProvider<ApplicationConfigInfo>
    {
        public ApplicationConfigInfo Provide()
        {
            ApplicationConfigInfo result = new();
            result.Load();
            return result;
        }

        public string Title => Lang.InfoProvider.ApplicationConfig.FileTitle;

        public string Description => string.Format(Lang.InfoProvider.ApplicationConfig.FileDescription, ApplicationConfigInfo.ConfigurationFilePath);
    }
}