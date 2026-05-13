using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal sealed class ManuscriptCalculatorForm : Form
    {
        private const int ResizeBorderThickness = 8;
        private const int WindowStyleThickFrame = 0x00040000;
        private const int WindowStyleMinimizeBox = 0x00020000;
        private const int WindowStyleMaximizeBox = 0x00010000;
        private const int WindowStyleCaption = 0x00C00000;
        private const int WindowStyleSysMenu = 0x00080000;

        private readonly List<ManuscriptLineControl> _lines = new List<ManuscriptLineControl>();
        private readonly BufferedPanel _rowsHost;
        private readonly ToolTip _toolTip;
        private PillButton _clearButton;
        private PillButton _hideButton;
        private ManuscriptLineControl _activeLine;
        private int _lastRowsWidth = -1;
        private bool _isActivated;

        public event EventHandler HideRequested;

        public ManuscriptCalculatorForm()
        {
            Text = "计算稿纸";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(920, 640);
            Size = new Size(1180, 820);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;
            DoubleBuffered = true;
            Padding = new Padding(1);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BufferedPanel root = new BufferedPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(18, 0, 18, 14);
            root.BackColor = Color.Transparent;
            Controls.Add(root);

            Panel titleBar = BuildTitleBar();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 92;

            BufferedPanel footer = BuildFooter();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 30;

            _rowsHost = new BufferedPanel();
            _rowsHost.Dock = DockStyle.Fill;
            _rowsHost.Margin = Padding.Empty;
            _rowsHost.Padding = new Padding(0, 0, 0, 0);
            _rowsHost.AutoScroll = true;
            _rowsHost.BackColor = Color.Transparent;
            _rowsHost.Resize += delegate { LayoutLines(false); };

            root.Controls.Add(_rowsHost);
            root.Controls.Add(footer);
            root.Controls.Add(titleBar);

            _toolTip = new ToolTip();
            _toolTip.SetToolTip(_clearButton, "清空全部公式");
            _toolTip.SetToolTip(_hideButton, "收起到托盘");

            EnsureTrailingBlankLine();
            SetActiveLine(_lines[0], true);
        }

        public void FocusActiveEditor()
        {
            if (_activeLine != null)
            {
                _activeLine.FocusEditor();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~(WindowStyleCaption | WindowStyleSysMenu);
                cp.Style |= WindowStyleThickFrame | WindowStyleMinimizeBox | WindowStyleMaximizeBox;
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            FocusActiveEditor();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _isActivated = true;
            if (IsHandleCreated)
            {
                NativeMethods.ApplyWindowChrome(Handle);
                Invalidate(true);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            _isActivated = false;
            Invalidate(true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeMethods.ApplyWindowChrome(Handle);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                RequestHide();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                CopyActiveResult();
                return true;
            }

            if (keyData == (Keys.Control | Keys.R))
            {
                ClearAllLines();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_NCCALCSIZE && m.WParam != IntPtr.Zero)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == NativeMethods.WM_NCACTIVATE)
            {
                m.Result = (IntPtr)1;
                return;
            }

            if (m.Msg == NativeMethods.WM_NCHITTEST && WindowState == FormWindowState.Normal)
            {
                base.WndProc(ref m);

                if ((int)m.Result == NativeMethods.HTCLIENT)
                {
                    Point screenPoint = new Point((short)((long)m.LParam & 0xFFFF), (short)(((long)m.LParam >> 16) & 0xFFFF));
                    Point clientPoint = PointToClient(screenPoint);
                    int hit = GetResizeHitTest(clientPoint);
                    if (hit != NativeMethods.HTCLIENT)
                    {
                        m.Result = (IntPtr)hit;
                    }
                }

                return;
            }

            if (m.Msg == NativeMethods.WM_DPICHANGED)
            {
                base.WndProc(ref m);
                LayoutLines(true);
                Invalidate(true);
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, UiPalette.CanvasTop, UiPalette.CanvasBottom, 90F))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Color borderColor = _isActivated ? UiPalette.AccentStart : UiPalette.WindowBorder;
            using (Pen pen = new Pen(borderColor))
            {
                e.Graphics.DrawLine(pen, 0, 1, 0, Height - 2);
                e.Graphics.DrawLine(pen, Width - 1, 1, Width - 1, Height - 2);
                e.Graphics.DrawLine(pen, 1, Height - 1, Width - 2, Height - 1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_toolTip != null)
                {
                    _toolTip.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private Panel BuildTitleBar()
        {
            BufferedPanel titleBar = new BufferedPanel();
            titleBar.BackColor = Color.Transparent;
            titleBar.MouseDown += BeginDrag;

            CalculatorGlyphBadge iconPlate = new CalculatorGlyphBadge();
            iconPlate.MouseDown += BeginDrag;
            iconPlate.Size = new Size(24, 22);

            Label title = new Label();
            title.AutoSize = true;
            title.Text = "计算稿纸";
            title.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            title.ForeColor = UiPalette.InkStrong;
            title.MouseDown += BeginDrag;

            _clearButton = CreateCommandButton("清空", Color.White, UiPalette.InkStrong, 24);
            _clearButton.Click += delegate { ClearAllLines(); };

            _hideButton = CreateCommandButton("✕", Color.White, UiPalette.InkStrong, 16);
            _hideButton.Click += delegate { RequestHide(); };

            titleBar.Controls.Add(iconPlate);
            titleBar.Controls.Add(title);
            titleBar.Controls.Add(_clearButton);
            titleBar.Controls.Add(_hideButton);

            titleBar.Resize += delegate
            {
                int right = titleBar.ClientSize.Width;

                iconPlate.Location = new Point(0, CenterY(titleBar.ClientSize.Height, iconPlate.Height));
                title.Location = new Point(32, CenterY(titleBar.ClientSize.Height, title.Height));

                _hideButton.Location = new Point(right - _hideButton.Width, CenterY(titleBar.ClientSize.Height, _hideButton.Height));
                _clearButton.Location = new Point(_hideButton.Left - 14 - _clearButton.Width, CenterY(titleBar.ClientSize.Height, _clearButton.Height));
            };

            return titleBar;
        }

        private BufferedPanel BuildFooter()
        {
            BufferedPanel footer = new BufferedPanel();
            footer.BackColor = Color.Transparent;
            footer.Height = 30;

            Label shortcuts = new Label();
            shortcuts.AutoSize = true;
            shortcuts.Text = "Enter 下一行    ↑↓ 切换    Ctrl+C 复制    Ctrl+R 清空    Esc 收起";
            shortcuts.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            shortcuts.ForeColor = UiPalette.InkSoft;

            footer.Controls.Add(shortcuts);

            footer.Resize += delegate
            {
                shortcuts.Location = new Point(4, 8);
            };

            return footer;
        }

        private static int CenterY(int containerHeight, int controlHeight)
        {
            return Math.Max(0, (containerHeight - controlHeight) / 2);
        }

        private PillButton CreateCommandButton(string text, Color backColor, Color foreColor, int extraPadding)
        {
            PillButton button = new PillButton();
            bool isAccent = backColor.ToArgb() == UiPalette.AccentStart.ToArgb();

            button.FillColor = backColor;
            button.HoverFillColor = isAccent ? ControlPaint.Light(backColor, 0.08F) : Color.FromArgb(238, 240, 246);
            button.PressedFillColor = isAccent ? ControlPaint.Dark(backColor, 0.06F) : Color.FromArgb(224, 227, 237);
            button.BorderColor = isAccent ? Color.Transparent : Color.FromArgb(210, 213, 224);
            button.TextColor = foreColor;
            button.Text = text;
            button.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            button.CornerRadius = 18;
            button.TabStop = false;

            Size measured = TextRenderer.MeasureText(text, button.Font);
            int width = Math.Max(64, measured.Width + extraPadding + 12);
            button.Size = new Size(width, 48);
            return button;
        }

        private int GetResizeHitTest(Point point)
        {
            bool left = point.X <= ResizeBorderThickness;
            bool right = point.X >= ClientSize.Width - ResizeBorderThickness;
            bool top = point.Y <= ResizeBorderThickness;
            bool bottom = point.Y >= ClientSize.Height - ResizeBorderThickness;

            if (left && top)
            {
                return NativeMethods.HTTOPLEFT;
            }

            if (right && top)
            {
                return NativeMethods.HTTOPRIGHT;
            }

            if (left && bottom)
            {
                return NativeMethods.HTBOTTOMLEFT;
            }

            if (right && bottom)
            {
                return NativeMethods.HTBOTTOMRIGHT;
            }

            if (left)
            {
                return NativeMethods.HTLEFT;
            }

            if (right)
            {
                return NativeMethods.HTRIGHT;
            }

            if (top)
            {
                return NativeMethods.HTTOP;
            }

            if (bottom)
            {
                return NativeMethods.HTBOTTOM;
            }

            return NativeMethods.HTCLIENT;
        }

        private void BeginDrag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.BeginWindowDrag(Handle);
            }
        }

        private void CopyActiveResult()
        {
            if (_activeLine == null)
            {
                return;
            }

            EvaluationState state = _activeLine.LastEvaluation;
            if (!state.Success)
            {
                return;
            }

            Clipboard.SetText(state.DisplayText);
        }

        private void ClearAllLines()
        {
            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                ManuscriptLineControl line = _lines[i];
                _rowsHost.Controls.Remove(line);
                line.Dispose();
            }

            _lines.Clear();
            _lastRowsWidth = -1;
            EnsureTrailingBlankLine();
            SetActiveLine(_lines[0], true);
            LayoutLines(true);
        }

        private void RequestHide()
        {
            EventHandler handler = HideRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void AddLine()
        {
            ManuscriptLineControl line = new ManuscriptLineControl();
            line.ExpressionChanged += OnLineExpressionChanged;
            line.LineFocused += OnLineFocused;
            line.EnterPressed += OnLineEnterPressed;
            line.NavigateUpRequested += OnLineNavigateUpRequested;
            line.NavigateDownRequested += OnLineNavigateDownRequested;
            line.DeleteRequested += OnLineDeleteRequested;

            _lines.Add(line);
            _rowsHost.Controls.Add(line);
            LayoutLines(true);
            ApplyLineStyles();
        }

        private void EnsureTrailingBlankLine()
        {
            if (_lines.Count == 0)
            {
                AddLine();
            }

            if (!_lines[_lines.Count - 1].IsEmpty)
            {
                AddLine();
            }

            while (_lines.Count >= 2 &&
                   _lines[_lines.Count - 1].IsEmpty &&
                   _lines[_lines.Count - 2].IsEmpty &&
                   _lines[_lines.Count - 1] != _activeLine)
            {
                RemoveLine(_lines[_lines.Count - 1], false);
            }
        }

        private void RemoveLine(ManuscriptLineControl line, bool updateFocus)
        {
            if (line == null || _lines.Count == 1)
            {
                return;
            }

            int index = _lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            _rowsHost.Controls.Remove(line);
            _lines.RemoveAt(index);
            line.Dispose();

            if (updateFocus)
            {
                int nextIndex = Math.Max(0, index - 1);
                if (_lines.Count > 0)
                {
                    SetActiveLine(_lines[nextIndex], true);
                }
            }

            EnsureTrailingBlankLine();
            LayoutLines(true);
        }

        private void LayoutLines(bool force)
        {
            int width = Math.Max(320, _rowsHost.ClientSize.Width - 2);
            if (!force && width == _lastRowsWidth)
            {
                return;
            }

            _lastRowsWidth = width;

            _rowsHost.SuspendLayout();

            int y = 0;
            foreach (ManuscriptLineControl line in _lines)
            {
                Rectangle bounds = new Rectangle(0, y, width, line.PreferredLineHeight);
                if (line.Bounds != bounds)
                {
                    line.Bounds = bounds;
                }

                y += line.PreferredLineHeight;
            }

            _rowsHost.AutoScrollMinSize = new Size(0, y + 8);
            _rowsHost.ResumeLayout();
        }

        private void ApplyLineStyles()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].ApplyVisualState(i, _lines[i] == _activeLine);
            }
        }

        private void SetActiveLine(ManuscriptLineControl line, bool focusEditor)
        {
            _activeLine = line;
            ApplyLineStyles();

            if (focusEditor && line != null)
            {
                line.FocusEditor();
            }
        }

        private void OnLineExpressionChanged(object sender, EventArgs e)
        {
            ManuscriptLineControl line = (ManuscriptLineControl)sender;
            EvaluationState state = ExpressionEvaluator.Evaluate(line.ExpressionText);
            line.SetEvaluation(state);
            EnsureTrailingBlankLine();
            LayoutLines(true);
        }

        private void OnLineFocused(object sender, EventArgs e)
        {
            SetActiveLine((ManuscriptLineControl)sender, false);
        }

        private void OnLineEnterPressed(object sender, EventArgs e)
        {
            ManuscriptLineControl line = (ManuscriptLineControl)sender;
            int index = _lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            EnsureTrailingBlankLine();

            if (index + 1 >= _lines.Count)
            {
                AddLine();
            }

            SetActiveLine(_lines[Math.Min(index + 1, _lines.Count - 1)], true);
        }

        private void OnLineNavigateUpRequested(object sender, EventArgs e)
        {
            int index = _lines.IndexOf((ManuscriptLineControl)sender);
            if (index > 0)
            {
                SetActiveLine(_lines[index - 1], true);
            }
        }

        private void OnLineNavigateDownRequested(object sender, EventArgs e)
        {
            int index = _lines.IndexOf((ManuscriptLineControl)sender);
            if (index >= 0 && index < _lines.Count - 1)
            {
                SetActiveLine(_lines[index + 1], true);
            }
        }

        private void OnLineDeleteRequested(object sender, EventArgs e)
        {
            ManuscriptLineControl line = (ManuscriptLineControl)sender;
            if (_lines.Count <= 1)
            {
                return;
            }

            RemoveLine(line, true);
        }
    }
}
