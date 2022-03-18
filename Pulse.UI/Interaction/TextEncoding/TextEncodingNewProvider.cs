using Pulse.Core;

namespace Pulse.UI
{
    public sealed class TextEncodingNewProvider : IInfoProvider<TextEncodingInfo>
    {
        public TextEncodingInfo Provide()
        {
            return TextEncodingInfo.CreateDefault();
        }

        public string Title => Lang.InfoProvider.TextEncoding.NewTitle;

        public string Description => Lang.InfoProvider.TextEncoding.NewDescription;
    }
}