using System;
using System.Windows;
using System.Windows.Media;

namespace Pulse.DirectX
{
    public class UiDxControl : FrameworkElement
    {
        public delegate void DrawSpritesDelegate(Device device, SpriteBatch spriteBatch, Rectangle clipRectangle);

        public delegate void DrawPrimitivesDelegate(Device device, RenderTarget target2D, Rectangle clipRectangle);

        public readonly RenderContainer RenderContainer;

        public event DrawSpritesDelegate DrawSprites;
        public event DrawPrimitivesDelegate DrawPrimitives;

        private Color _backgroundColor = Colors.LightGray;
        private System.Windows.Media.SolidColorBrush _backgroundBrush;

        internal DrawingContext ActiveContext { get; private set; }

        public UiDxControl()
        {
            RenderContainer = new RenderContainer(this);
            _backgroundBrush = CreateBackgroundBrush(_backgroundColor);

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
            SnapsToDevicePixels = true;
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value)
                    return;

                _backgroundColor = value;
                _backgroundBrush = CreateBackgroundBrush(value);
                InvalidateVisual();
            }
        }

        public void Refresh()
        {
            if (Dispatcher.CheckAccess())
                InvalidateVisual();
            else
                Dispatcher.BeginInvoke(new Action(InvalidateVisual));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Rect bounds = new(RenderSize);
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            drawingContext.DrawRectangle(_backgroundBrush, null, bounds);
            drawingContext.PushClip(new RectangleGeometry(bounds));

            ActiveContext = drawingContext;
            try
            {
                Rectangle clip = new(0, 0, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));

                DrawSpritesDelegate drawSprites = DrawSprites;
                if (drawSprites != null)
                    drawSprites(Device.Instance, new SpriteBatch(drawingContext), clip);

                DrawPrimitives?.Invoke(Device.Instance, RenderContainer.BackBuffer.Target2D, clip);
            }
            finally
            {
                ActiveContext = null;
                drawingContext.Pop();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RenderContainer.RaiseReseted();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Rect bounds = new(RenderSize);
            return bounds.Contains(hitTestParameters.HitPoint)
                ? new PointHitTestResult(this, hitTestParameters.HitPoint)
                : null;
        }

        internal void DrawRectangle(RectangleF rectangle, SolidColorBrush brush, float strokeWidth)
        {
            DrawingContext context = ActiveContext;
            if (context == null)
                return;

            Rect rect = new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            if (strokeWidth <= 0.01f)
                context.DrawRectangle(brush.Brush, null, rect);
            else
                context.DrawRectangle(null, new Pen(brush.Brush, strokeWidth), rect);
        }

        private static System.Windows.Media.SolidColorBrush CreateBackgroundBrush(Color color)
        {
            System.Windows.Media.SolidColorBrush brush = new(color);
            brush.Freeze();
            return brush;
        }
    }
}
