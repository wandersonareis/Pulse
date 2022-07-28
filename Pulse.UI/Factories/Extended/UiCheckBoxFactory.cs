namespace Pulse.UI
{
    public static class UiCheckBoxFactory
    {
        public static UiCheckBox Create(object content, bool? isChecked)
        {
            return new()
            {
                Content = content,
                IsChecked = isChecked
            };
        }
    }
}