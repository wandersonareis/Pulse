using System.Windows.Media;

namespace Pulse.DirectX
{
    public sealed class BackBuffer
    {
        private readonly UiDxControl _owner;

        internal BackBuffer(UiDxControl owner)
        {
            _owner = owner;
            Target2D = new RenderTarget(owner);
        }

        public RenderTarget Target2D { get; }

        public Color BackgroundColor
        {
            get => _owner.BackgroundColor;
            set => _owner.BackgroundColor = value;
        }
    }
}
