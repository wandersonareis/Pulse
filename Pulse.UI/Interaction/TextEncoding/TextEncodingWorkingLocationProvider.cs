using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingWorkingLocationProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.Load();
        }

        public string Title => Lang.InfoProvider.TextEncoding.WorkingLocationTitle;

        public string Description => Lang.InfoProvider.TextEncoding.WorkingLocationDescription;
    }
}