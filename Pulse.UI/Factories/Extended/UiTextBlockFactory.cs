using Pulse.Core;

namespace Pulse.UI
{
    public static class UiTextBlockFactory
    {
        public static UiTextBlock Create(string text)
        {
            Exceptions.CheckArgumentNull(text, "text");

            UiTextBlock textBlock = new() {Text = text};

            return textBlock;
        }
    }
}