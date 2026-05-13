using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal sealed class CalculatorGlyphBadge : Control
    {
        public CalculatorGlyphBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint, true);

            BackColor = Color.Transparent;
            Size = new Size(30, 28);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = UiHelpers.CreateRoundedRectangle(rect, 10))
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, UiPalette.AccentStart, UiPalette.AccentEnd, 90F))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                e.Graphics.FillPath(brush, path);

                e.Graphics.FillRectangle(whiteBrush, 7, 8, 12, 2);
                e.Graphics.FillRectangle(whiteBrush, 12, 4, 2, 10);
                e.Graphics.FillRectangle(whiteBrush, 17, 10, 6, 2);
                e.Graphics.FillRectangle(whiteBrush, 17, 15, 7, 2);

                GraphicsState state = e.Graphics.Save();
                e.Graphics.TranslateTransform(8F, 21F);
                e.Graphics.RotateTransform(-28F);
                e.Graphics.FillRectangle(whiteBrush, 0, 0, 11, 2);
                e.Graphics.Restore(state);
            }
        }
    }
}
