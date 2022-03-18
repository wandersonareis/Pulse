namespace Pulse.UI
{
    public sealed class LocalizatorEnvironmentConfigurationProvider : IInfoProvider<LocalizatorEnvironmentInfo>
    {
        public LocalizatorEnvironmentInfo Provide()
        {
            LocalizatorEnvironmentInfo value = InteractionService.Configuration.Provide().LocalizatorEnvironment;
            value.Validate();
            return value;
        }

        public string Title => "LocalizatorEnvironmentConfigurationProvider";

        public string Description => string.Empty;
    }
}