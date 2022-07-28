namespace Pulse.UI
{
    public static class UiGridSplitterFactory
    {
        public static UiGridSplitter Create()
        {
            UiGridSplitter splitter = new() {Width = 5};

            return splitter;
        }
    }
}