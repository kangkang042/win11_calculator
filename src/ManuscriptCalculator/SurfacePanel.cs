using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal sealed class SurfacePanel : Panel
    {
        private int _cornerRadius = 20;
        private Color _fillColor = Color.White;
        private Color _fillColor2 = Color.White;
        private Color _strokeColor = Color.Transparent;

        public SurfacePanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                UiHelpers.ApplyRoundedRegion(this, value);
                Invalidate();
            }
        }

        public Color FillColor
        {
            get { return _fillColor; }
            set
            {
                _fillColor = value;
                Invalidate();
            }
        }

        public Color FillColor2
        {
            get { return _fillColor2; }
            set
            {
                _fillColor2 = value;
                Invalidate();
            }
        }

        public Color StrokeColor
        {
            get { return _strokeColor; }
            set
            {
                _strokeColor = value;
                Invalidate();
            }
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UiHelpers.ApplyRoundedRegion(this, CornerRadius);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = UiHelpers.CreateRoundedRectangle(rect, CornerRadius))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, FillColor, FillColor2, 90f))
            {
                e.Graphics.FillPath(brush, path);

                if (StrokeColor.A > 0)
                {
                    using (Pen pen = new Pen(StrokeColor))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
