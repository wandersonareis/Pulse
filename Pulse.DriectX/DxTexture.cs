using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Pulse.DirectX
{
    public sealed class DxTexture : IDisposable
    {
        public BitmapSource Bitmap { get; }
        public bool IsCube { get; }

        public int Width => Bitmap.PixelWidth;
        public int Height => Bitmap.PixelHeight;

        public TextureDescriptor Descriptor2D => new(Width, Height);

        public DxTexture(BitmapSource bitmap, bool isCube = false)
        {
            Bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            IsCube = isCube;
        }

        public void Dispose()
        {
        }

        public void Draw(Device device, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, float layerDepth)
        {
            Rectangle source = sourceRectangle ?? new Rectangle(0, 0, Width, Height);

            int left = Math.Max(0, source.X);
            int top = Math.Max(0, source.Y);
            int right = Math.Min(Width, source.X + source.Width);
            int bottom = Math.Min(Height, source.Y + source.Height);
            if (right <= left || bottom <= top)
                return;

            Rect destination = new(position.X + (left - source.X), position.Y + (top - source.Y), right - left, bottom - top);
            spriteBatch.DrawImage(Bitmap, destination, new Int32Rect(left, top, right - left, bottom - top));
        }

        public void Draw(Device device, SpriteBatch spriteBatch, Vector2 position, Rectangle sourceRectangle, float layerDepth, Rectangle viewport)
        {
            position.X -= viewport.Left;
            if (position.X < 0)
            {
                sourceRectangle.Left -= (int)position.X;
                if (sourceRectangle.Width < 1)
                    return;
                position.X = 0;
            }

            int ox = viewport.Width - (int)(position.X + sourceRectangle.Width);
            if (ox < 0)
            {
                sourceRectangle.Right += ox;
                if (sourceRectangle.Width < 1)
                    return;
            }

            position.Y -= viewport.Top;
            if (position.Y < 0)
            {
                sourceRectangle.Top -= (int)position.Y;
                if (sourceRectangle.Height < 1)
                    return;
                position.Y = 0;
            }

            int oy = viewport.Height - (int)(position.Y + sourceRectangle.Height);
            if (oy < 0)
            {
                sourceRectangle.Bottom += oy;
                if (sourceRectangle.Height < 1)
                    return;
            }

            Draw(device, spriteBatch, position, sourceRectangle, layerDepth);
        }
    }
}
