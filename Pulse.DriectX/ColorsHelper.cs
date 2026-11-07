using System.Windows.Media;

namespace Pulse.DirectX
{
    public static class DxColorsHelper
    {
        public static Color4 ToColor4(this Color color)
        {
            const float max = byte.MaxValue;
            return new(color.R / max, color.G / max, color.B / max, color.A / max);
        }
    }
}
