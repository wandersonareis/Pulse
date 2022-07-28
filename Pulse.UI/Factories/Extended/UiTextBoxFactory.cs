namespace Pulse.UI
{
    public static class UiTextBoxFactory
    {
        public static UiTextBox Create()
        {
            return new();
        }

        public static UiTextBox Create(string text)
        {
            return new() {Text = text};
        }
    }
}