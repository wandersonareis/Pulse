namespace Pulse.UI
{
    public static class UiIntegerUpDownFactory
    {
        public static UiIntegerUpDown Create(int? minimum, int? maximum)
        {
            return new()
            {
                Minimum = minimum,
                Maximum = maximum
            };
        }
    }
}