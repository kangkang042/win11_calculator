using System;
using System.Drawing;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal sealed class ManuscriptLineControl : UserControl
    {
        private const int HorizontalTextPadding = 8;

        private readonly TextBox _editor;
        private readonly Label _placeholder;
        private readonly Label _result;
        private readonly ToolTip _copyToolTip;
        private bool _isActive;
        private bool _isHovered;


        public ManuscriptLineControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Height = PreferredLineHeight;
            Margin = Padding.Empty;
            Padding = new Padding(24, 12, 24, 12);

            _editor = new TextBox();
            _editor.BorderStyle = BorderStyle.None;
            _editor.Multiline = false;
            _editor.AutoSize = false;
            _editor.AcceptsReturn = false;
            _editor.WordWrap = false;
            _editor.ScrollBars = ScrollBars.None;
            _editor.Font = new Font("Cascadia Mono", 19F, FontStyle.Regular, GraphicsUnit.Point);
            _editor.ForeColor = UiPalette.InkStrong;
            _editor.BackColor = UiPalette.PaperLight;
            _editor.Location = new Point(26, 22);
            _editor.Size = new Size(760, 34);
            _editor.TextChanged += OnTextChanged;
            _editor.GotFocus += OnFocusChanged;
            _editor.LostFocus += OnFocusChanged;
            _editor.HandleCreated += delegate { ApplyEditorMargins(); };
            _editor.MouseDown += delegate { RaiseLineFocused(); };
            _editor.KeyDown += OnEditorKeyDown;

            _placeholder = new Label();
            _placeholder.AutoSize = false;
            _placeholder.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
            _placeholder.ForeColor = UiPalette.InkSoft;
            _placeholder.Text = "输入公式";
            _placeholder.TextAlign = ContentAlignment.MiddleLeft;
            _placeholder.Location = new Point(26, 20);
            _placeholder.Size = new Size(360, 32);
            _placeholder.Click += delegate { FocusEditor(); };

            _result = new Label();
            _result.AutoSize = false;
            _result.AutoEllipsis = true;
            _result.Font = new Font("Cascadia Mono", 20F, FontStyle.Bold, GraphicsUnit.Point);
            _result.ForeColor = Color.FromArgb(22, 22, 22);
            _result.TextAlign = ContentAlignment.MiddleRight;
            _result.Text = "=";
            _result.Click += OnResultClicked;

            _copyToolTip = new ToolTip();
            _copyToolTip.InitialDelay = 0;
            _copyToolTip.ReshowDelay = 0;
            _copyToolTip.AutoPopDelay = 1200;
            _copyToolTip.ShowAlways = true;

            Controls.Add(_placeholder);
            Controls.Add(_editor);
            Controls.Add(_result);

            Resize += delegate { LayoutInternal(); };
            LayoutInternal();
            SetEvaluation(EvaluationState.Empty());
        }

        public event EventHandler ExpressionChanged;

        public event EventHandler LineFocused;

        public event EventHandler EnterPressed;

        public event EventHandler NavigateUpRequested;

        public event EventHandler NavigateDownRequested;

        public event EventHandler DeleteRequested;

        public EvaluationState LastEvaluation { get; private set; }

        public int PreferredLineHeight
        {
            get { return 78; }
        }

        public string ExpressionText
        {
            get { return _editor.Text; }
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(_editor.Text); }
        }

        public void FocusEditor()
        {
            _editor.Focus();
            _editor.SelectionStart = _editor.TextLength;
        }

        public void SetEvaluation(EvaluationState state)
        {
            LastEvaluation = state;

            if (!state.HasExpression)
            {
                _result.Text = "=";
                _result.ForeColor = UiPalette.InkSoft;
            }
            else if (state.Success)
            {
                _result.Text = "= " + state.DisplayText;
                _result.ForeColor = Color.FromArgb(22, 22, 22);
            }
            else
            {
                _result.Text = state.ErrorMessage;
                _result.ForeColor = UiPalette.Error;
            }

            UpdatePlaceholderVisibility();
        }

        public void ApplyVisualState(int index, bool isActive)
        {
            _isActive = isActive;

            Color backColor;
            if (isActive)
            {
                backColor = UiPalette.ActivePaper;
            }
            else if (_isHovered)
            {
                backColor = Color.FromArgb(247, 248, 251);
            }
            else if (index % 2 == 0)
            {
                backColor = UiPalette.PaperLight;
            }
            else
            {
                backColor = UiPalette.PaperAlternate;
            }

            BackColor = backColor;
            _editor.BackColor = backColor;
            _placeholder.BackColor = backColor;
            _result.BackColor = backColor;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            RaiseLineFocused();
            FocusEditor();
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
            Invalidate();
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
            base.OnPaint(e);

            using (Pen linePen = new Pen(Color.FromArgb(232, 233, 237)))
            {
                e.Graphics.DrawLine(linePen, 0, Height - 1, Width, Height - 1);
            }

            if (_isActive)
            {
                using (SolidBrush accentBrush = new SolidBrush(UiPalette.AccentStart))
                {
                    e.Graphics.FillRectangle(accentBrush, 0, 0, 3, Height);
                }
            }
        }

        private void LayoutInternal()
        {
            int left = 26;
            int rightPadding = 22;
            int resultWidth = Math.Min(380, Math.Max(190, Width / 3));
            int editorWidth = Math.Max(140, Width - left - rightPadding - resultWidth - 22);
            int editorHeight = Math.Max(40, _editor.PreferredHeight + 8);
            int editorTop = Math.Max(12, (Height - editorHeight) / 2);
            int placeholderTop = editorTop;

            _editor.Location = new Point(left, editorTop);
            _editor.Size = new Size(editorWidth, editorHeight);

            _placeholder.Location = new Point(left + HorizontalTextPadding, placeholderTop);
            _placeholder.Size = new Size(Math.Max(120, editorWidth - HorizontalTextPadding), editorHeight);

            _result.Location = new Point(Width - resultWidth - rightPadding, 10);
            _result.Size = new Size(resultWidth, 46);
        }

        private void ApplyEditorMargins()
        {
            if (_editor.IsHandleCreated)
            {
                NativeMethods.SetEditMargins(_editor.Handle, HorizontalTextPadding, HorizontalTextPadding);
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            UpdatePlaceholderVisibility();

            EventHandler handler = ExpressionChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdatePlaceholderVisibility()
        {
            bool isEmpty = string.IsNullOrWhiteSpace(_editor.Text);
            _placeholder.Visible = isEmpty && !_editor.Focused;
            _placeholder.ForeColor = _editor.Focused ? Color.FromArgb(172, 161, 142) : UiPalette.InkSoft;
        }

        private void OnFocusChanged(object sender, EventArgs e)
        {
            RaiseLineFocused();
            UpdatePlaceholderVisibility();
        }

        private void RaiseLineFocused()
        {
            EventHandler handler = LineFocused;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                EventHandler handler = EnterPressed;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                EventHandler handler = NavigateUpRequested;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                EventHandler handler = NavigateDownRequested;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                return;
            }

            if (e.KeyCode == Keys.Back && string.IsNullOrWhiteSpace(_editor.Text))
            {
                EventHandler handler = DeleteRequested;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        private void OnResultClicked(object sender, EventArgs e)
        {
            if (LastEvaluation != null && LastEvaluation.Success)
            {
                Clipboard.SetText(LastEvaluation.DisplayText);
                _copyToolTip.Hide(this);
                _copyToolTip.Show("已复制", this, _result.Left + _result.Width - 76, _result.Top - 8, 1100);
            }
        }
    }
}
