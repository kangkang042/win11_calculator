using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ManuscriptCalculator
{
    internal static class AppIconFactory
    {
        public static Icon CreateCalculatorIcon()
        {
            using (Bitmap bitmap = new Bitmap(64, 64))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                Rectangle body = new Rectangle(4, 4, 56, 56);
                using (GraphicsPath path = UiHelpers.CreateRoundedRectangle(body, 16))
                using (LinearGradientBrush brush = new LinearGradientBrush(body, UiPalette.AccentStart, UiPalette.AccentEnd, 90f))
                using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                {
                    graphics.FillPath(brush, path);

                    using (Pen pen = new Pen(Color.FromArgb(235, 255, 255, 255), 2f))
                    {
                        graphics.DrawPath(pen, path);
                    }

                    graphics.FillRectangle(whiteBrush, 15, 14, 16, 6);
                    graphics.FillRectangle(whiteBrush, 20, 9, 6, 16);
                    graphics.FillRectangle(whiteBrush, 38, 15, 11, 5);
                    graphics.FillRectangle(whiteBrush, 36, 26, 15, 5);
                    graphics.FillRectangle(whiteBrush, 36, 38, 15, 5);
                    graphics.FillRectangle(whiteBrush, 36, 50, 15, 5);

                    graphics.FillEllipse(whiteBrush, 15, 34, 7, 7);
                    graphics.FillEllipse(whiteBrush, 24, 34, 7, 7);
                    graphics.FillEllipse(whiteBrush, 15, 45, 7, 7);
                    graphics.FillEllipse(whiteBrush, 24, 45, 7, 7);
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
    }
}
