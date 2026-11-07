using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pulse.DirectX
{
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static readonly Vector2 Zero = new(0, 0);
        public static readonly Vector2 One = new(1, 1);
    }

    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Left
        {
            get => X;
            set => X = value;
        }

        public int Top
        {
            get => Y;
            set => Y = value;
        }

        public int Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        public int Bottom
        {
            get => Y + Height;
            set => Height = value - Y;
        }
    }

    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public struct Color4
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public Color4(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static readonly Color4 White = new(1, 1, 1, 1);
    }

    public struct Matrix
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float M31;
        public float M32;

        public static readonly Matrix Identity = new() { M11 = 1, M22 = 1 };

        public static Matrix Scaling(float scale) => new() { M11 = scale, M22 = scale };
    }

    public enum SpriteSortMode
    {
        Deferred,
        Immediate,
        Texture,
        BackToFront,
        FrontToBack
    }

    public enum SpriteEffects
    {
        None,
        FlipHorizontally,
        FlipVertically
    }

    public sealed class Device
    {
        public static readonly Device Instance = new();

        private Device()
        {
        }
    }

    public sealed class SamplerState
    {
    }

    public sealed class SamplerStateCollection
    {
        public SamplerState PointClamp { get; } = new();
        public SamplerState LinearClamp { get; } = new();
    }

    public sealed class GraphicsDevice
    {
        public static readonly GraphicsDevice Instance = new();

        public SamplerStateCollection SamplerStates { get; } = new();

        private GraphicsDevice()
        {
        }
    }

    public readonly struct TextureDescriptor
    {
        public int Width { get; }
        public int Height { get; }

        public TextureDescriptor(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public sealed class SpriteBatch
    {
        private readonly DrawingContext _context;
        private int _pushed;

        internal SpriteBatch(DrawingContext context)
        {
            _context = context;
        }

        public void Begin()
        {
        }

        public void Begin(SpriteSortMode sortMode, object blendState, SamplerState samplerState, object depthStencilState, object rasterizerState, object effect, Matrix transform)
        {
            if (transform.M11 == 1 && transform.M22 == 1 && transform.M31 == 0 && transform.M32 == 0)
                return;

            _context.PushTransform(new MatrixTransform(transform.M11, transform.M12, transform.M21, transform.M22, transform.M31, transform.M32));
            _pushed++;
        }

        public void End()
        {
            if (_pushed <= 0)
                return;

            _context.Pop();
            _pushed--;
        }

        internal void DrawImage(BitmapSource image, Rect destination, Int32Rect source)
        {
            CroppedBitmap cropped = new(image, source);
            _context.DrawImage(cropped, destination);
        }
    }

    public sealed class SolidColorBrush : IDisposable
    {
        internal System.Windows.Media.SolidColorBrush Brush { get; }

        public SolidColorBrush(RenderTarget target, Color4 color, object options)
        {
            Brush = new System.Windows.Media.SolidColorBrush(Color.FromArgb(ToByte(color.A), ToByte(color.R), ToByte(color.G), ToByte(color.B)));
            Brush.Freeze();
        }

        private static byte ToByte(float value)
        {
            return (byte)Math.Round(Math.Clamp(value, 0f, 1f) * byte.MaxValue);
        }

        public void Dispose()
        {
        }
    }

    public sealed class RenderTarget
    {
        private readonly UiDxControl _owner;

        internal RenderTarget(UiDxControl owner)
        {
            _owner = owner;
        }

        public void BeginDraw()
        {
        }

        public void EndDraw()
        {
        }

        public void FillRectangle(RectangleF rectangle, SolidColorBrush brush)
        {
            _owner.DrawRectangle(rectangle, brush, 0);
        }

        public void DrawRectangle(RectangleF rectangle, SolidColorBrush brush, float strokeWidth)
        {
            _owner.DrawRectangle(rectangle, brush, strokeWidth);
        }
    }
}
