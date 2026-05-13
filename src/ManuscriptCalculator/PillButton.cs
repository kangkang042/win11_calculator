using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal sealed class PillButton : Control
    {
        private bool _isHovered;
        private bool _isPressed;
        private int _cornerRadius = 10;
        private Color _fillColor = Color.White;
        private Color _hoverFillColor = Color.White;
        private Color _pressedFillColor = Color.White;
        private Color _borderColor = Color.Transparent;
        private Color _textColor = Color.Black;

        public PillButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = UiPalette.CanvasTop;
            Size = new Size(88, 34);
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; Invalidate(); }
        }

        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; Invalidate(); }
        }

        public Color HoverFillColor
        {
            get { return _hoverFillColor; }
            set { _hoverFillColor = value; Invalidate(); }
        }

        public Color PressedFillColor
        {
            get { return _pressedFillColor; }
            set { _pressedFillColor = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_isPressed)
            {
                _isPressed = false;
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
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

            Color fill;
            if (!Enabled)
            {
                fill = Color.FromArgb(235, 235, 235);
            }
            else if (_isPressed)
            {
                fill = PressedFillColor;
            }
            else if (_isHovered)
            {
                fill = HoverFillColor;
            }
            else
            {
                fill = FillColor;
            }

            float scale = _isPressed ? 0.97f : 1f;
            if (scale < 1f)
            {
                int dw = (int)(rect.Width * (1f - scale) / 2f);
                int dh = (int)(rect.Height * (1f - scale) / 2f);
                rect.Inflate(-dw, -dh);
            }

            using (GraphicsPath path = UiHelpers.CreateRoundedRectangle(rect, CornerRadius))
            using (SolidBrush brush = new SolidBrush(fill))
            {
                e.Graphics.FillPath(brush, path);

                if (BorderColor.A > 0)
                {
                    using (Pen pen = new Pen(BorderColor))
                    {
                        pen.Alignment = PenAlignment.Inset;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }

            Rectangle textRect = new Rectangle(0, 0, Width, Height);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                textRect,
                Enabled ? TextColor : Color.FromArgb(145, 145, 145),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding);
        }
    }
}
