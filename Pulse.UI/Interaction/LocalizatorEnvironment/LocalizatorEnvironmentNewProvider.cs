using System;

namespace Pulse.UI
{
    public sealed class LocalizatorEnvironmentNewProvider : IInfoProvider<LocalizatorEnvironmentInfo>
    {
        private const string DefaultPatherUrl = @"http://ff13.ffrtt.ru/rus/patcher.zip";
        private const string DefaultTranslationUrl = @"http://ff13.ffrtt.ru/rus/translation.zip";

        public LocalizatorEnvironmentInfo Provide()
        {
            Version version = new(1,0,0,0);
            string[] patherUrls = {DefaultPatherUrl};
            string[] translationUrls = {DefaultTranslationUrl};
            return new(version, patherUrls, translationUrls);
        }

        public string Title => "LocalizatorEnvironmentNewProvider";

        public string Description => string.Empty;
    }
}