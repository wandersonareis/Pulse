using System;

namespace Pulse.DirectX
{
    public sealed class RenderContainer
    {
        public readonly Device Device = Device.Instance;
        public readonly GraphicsDevice GraphicsDevice = GraphicsDevice.Instance;

        public BackBuffer BackBuffer { get; }

        public event Action<RenderContainer> Reseted;

        internal RenderContainer(UiDxControl owner)
        {
            BackBuffer = new BackBuffer(owner);
        }

        internal void RaiseReseted()
        {
            Reseted?.Invoke(this);
        }
    }
}
