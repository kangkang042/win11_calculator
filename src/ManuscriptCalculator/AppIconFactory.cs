using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManuscriptCalculator
{
    internal static class AppIconFactory
    {
        private static readonly Color AccentStart = Color.FromArgb(255, 193, 7);
        private static readonly Color AccentEnd = Color.FromArgb(255, 193, 7);

        public static Icon CreateCalculatorIcon()
        {
            using (Bitmap bitmap = new Bitmap(64, 64))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                Rectangle body = new Rectangle(4, 4, 56, 56);
                using (GraphicsPath path = CreateRoundedRectangle(body, 16))
                using (LinearGradientBrush brush = new LinearGradientBrush(body, AccentStart, AccentEnd, 90f))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                {
                    graphics.FillPath(brush, path);

                    // 等号
                    int cx = 32, cy = 32, barW = 20, barH = 5, gap = 7;
                    graphics.FillRectangle(whiteBrush, cx - barW / 2, cy - gap / 2 - barH, barW, barH);
                    graphics.FillRectangle(whiteBrush, cx - barW / 2, cy + gap / 2, barW, barH);
                }

                IntPtr hIcon = bitmap.GetHicon();
                try
                {
                    Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
                    return icon;
                }
                finally
                {
                    NativeMethods.DestroyIcon(hIcon);
                }
            }
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
