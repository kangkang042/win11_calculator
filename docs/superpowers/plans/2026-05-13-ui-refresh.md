# 计算稿纸界面优化实施计划

> **For agentic workers:** 使用 superpowers:subagent-driven-development 或 superpowers:executing-plans 按任务逐项实施。步骤使用 checkbox (`- [ ]`) 语法追踪。

**目标：** 在不变更布局结构的前提下，将配色从暖色调切换为中性冷色调，简化标题栏和底部栏，优化行控件视觉并增加微交互反馈。

**架构：** 纯 WinForms 桌面应用，自定义自绘 UI。改动集中在 UiPalette.cs（颜色常量）、ManuscriptCalculatorForm.cs（标题栏/底部栏/窗口激活边框）、ManuscriptLineControl.cs（行控件/闪烁反馈）、PillButton.cs（按钮动画）。

**技术栈：** C# .NET Framework 4.x，Windows Forms，System.Drawing

---

### Task 1: 配色方案切换

**文件：**
- 修改：`src/ManuscriptCalculator/UiPalette.cs`

- [ ] **Step 1: 更新 UiPalette 所有色值**

将 `UiPalette.cs` 的全部静态颜色字段替换为以下内容：

```csharp
using System.Drawing;

namespace ManuscriptCalculator
{
    internal static class UiPalette
    {
        public static readonly Color CanvasTop = Color.FromArgb(248, 249, 251);
        public static readonly Color CanvasBottom = Color.FromArgb(242, 243, 246);
        public static readonly Color WindowBorder = Color.FromArgb(229, 230, 234);
        public static readonly Color AccentStart = Color.FromArgb(74, 108, 247);
        public static readonly Color AccentEnd = Color.FromArgb(107, 138, 255);
        public static readonly Color InkStrong = Color.FromArgb(26, 29, 38);
        public static readonly Color InkMuted = Color.FromArgb(136, 140, 150);
        public static readonly Color InkSoft = Color.FromArgb(176, 180, 188);
        public static readonly Color PaperLight = Color.FromArgb(255, 255, 255);
        public static readonly Color PaperAlternate = Color.FromArgb(250, 251, 252);
        public static readonly Color ActivePaper = Color.FromArgb(238, 241, 250);
        public static readonly Color Error = Color.FromArgb(209, 82, 79);
    }
}
```

- [ ] **Step 2: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

预期：编译通过，无错误。`dist\ManuscriptCalculator.exe` 生成成功。

- [ ] **Step 3: 提交**

```bash
git add src/ManuscriptCalculator/UiPalette.cs
git commit -m "$(cat <<'EOF'
feat: 配色从暖色调切换为中性冷色调

Canvas/WindowBorder/Accent/Ink/Paper 全系列色值更新。

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 2: 标题栏简化

**文件：**
- 修改：`src/ManuscriptCalculator/ManuscriptCalculatorForm.cs` — BuildTitleBar() 方法（第 210-282 行）和构造函数中 titleBar.Height（第 55 行）

- [ ] **Step 1: 将 titleBar.Height 从 80 改为 56**

在构造函数中（约第 55 行），将：

```csharp
titleBar.Height = 80;
```

改为：

```csharp
titleBar.Height = 56;
```

- [ ] **Step 2: 重写 BuildTitleBar() 方法**

将整个 `BuildTitleBar()` 方法替换为以下内容：

```csharp
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
    title.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
    title.ForeColor = UiPalette.InkStrong;
    title.MouseDown += BeginDrag;

    _copyButton = CreateCommandButton("复制结果", Color.White, UiPalette.InkStrong, 18);
    _copyButton.Click += delegate { CopyActiveResult(); };

    _clearButton = CreateCommandButton("清空", Color.White, UiPalette.InkStrong, 18);
    _clearButton.Click += delegate { ClearAllLines(); };

    _hideButton = CreateCommandButton("收起", Color.White, UiPalette.InkStrong, 18);
    _hideButton.Click += delegate { RequestHide(); };

    titleBar.Controls.Add(iconPlate);
    titleBar.Controls.Add(title);
    titleBar.Controls.Add(_copyButton);
    titleBar.Controls.Add(_clearButton);
    titleBar.Controls.Add(_hideButton);

    titleBar.Resize += delegate
    {
        int right = titleBar.ClientSize.Width;

        iconPlate.Location = new Point(0, CenterY(titleBar.ClientSize.Height, iconPlate.Height));
        title.Location = new Point(32, CenterY(titleBar.ClientSize.Height, title.Height));

        _hideButton.Location = new Point(right - _hideButton.Width, CenterY(titleBar.ClientSize.Height, _hideButton.Height));
        _clearButton.Location = new Point(_hideButton.Left - 8 - _clearButton.Width, CenterY(titleBar.ClientSize.Height, _clearButton.Height));
        _copyButton.Location = new Point(_clearButton.Left - 8 - _copyButton.Width, CenterY(titleBar.ClientSize.Height, _copyButton.Height));
    };

    return titleBar;
}
```

- [ ] **Step 3: 简化 CreateCommandButton 样式**

将 `CreateCommandButton` 方法中 CornerRadius 相关行（约第 328-341 行），修改按钮属性：

在 `CreateCommandButton` 方法内，找到 `PillButton button = new PillButton();` 之后的设置行，将：

```csharp
button.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
```

改为：

```csharp
button.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
button.CornerRadius = 10;
```

对 `Size` 计算行进行修改，将：

```csharp
int width = Math.Max(38, measured.Width + extraPadding);
```

改为：

```csharp
int width = Math.Max(34, measured.Width + extraPadding);
```

- [ ] **Step 4: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

- [ ] **Step 5: 提交**

```bash
git add src/ManuscriptCalculator/ManuscriptCalculatorForm.cs
git commit -m "$(cat <<'EOF'
feat: 简化标题栏，降低高度和视觉重量

移除 SurfacePanel 品牌卡片，标题字体缩小，按钮更轻量化。

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 3: 行控件视觉优化

**文件：**
- 修改：`src/ManuscriptCalculator/ManuscriptLineControl.cs`

- [ ] **Step 1: 缩小 PreferredLineHeight 从 82 到 68**

将第 96-99 行的属性：

```csharp
public int PreferredLineHeight
{
    get { return 82; }
}
```

改为：

```csharp
public int PreferredLineHeight
{
    get { return 68; }
}
```

- [ ] **Step 2: 调整 LayoutInternal 内边距**

将 `LayoutInternal` 方法中（第 200-218 行）的 `editorTop` 计算和相关定位调整。将：

```csharp
int editorTop = Math.Max(14, (Height - editorHeight) / 2);
int placeholderTop = editorTop - 1;

_editor.Location = new Point(left, editorTop);
_editor.Size = new Size(editorWidth, editorHeight);

_placeholder.Location = new Point(left + HorizontalTextPadding, placeholderTop);
_placeholder.Size = new Size(Math.Max(120, editorWidth - HorizontalTextPadding), editorHeight);

_result.Location = new Point(Width - resultWidth - rightPadding, 14);
_result.Size = new Size(resultWidth, 50);
```

改为：

```csharp
int editorTop = Math.Max(10, (Height - editorHeight) / 2);
int placeholderTop = editorTop;

_editor.Location = new Point(left, editorTop);
_editor.Size = new Size(editorWidth, editorHeight);

_placeholder.Location = new Point(left + HorizontalTextPadding, placeholderTop);
_placeholder.Size = new Size(Math.Max(120, editorWidth - HorizontalTextPadding), editorHeight);

_result.Location = new Point(Width - resultWidth - rightPadding, 10);
_result.Size = new Size(resultWidth, 46);
```

- [ ] **Step 3: 激活左侧条颜色改用 AccentStart，宽度从 4 到 3**

在 `OnPaint` 方法中（第 180-198 行），将激活指示条绘制代码：

```csharp
if (_isActive)
{
    using (SolidBrush accentBrush = new SolidBrush(Color.FromArgb(255, 168, 64)))
    {
        e.Graphics.FillRectangle(accentBrush, 0, 0, 4, Height);
    }
}
```

改为：

```csharp
if (_isActive)
{
    using (SolidBrush accentBrush = new SolidBrush(UiPalette.AccentStart))
    {
        e.Graphics.FillRectangle(accentBrush, 0, 0, 3, Height);
    }
}
```

- [ ] **Step 4: 行间分隔线从双线改为单线**

在 `OnPaint` 方法中（第 180-198 行），将双线绘制：

```csharp
using (Pen topPen = new Pen(Color.FromArgb(235, 231, 223)))
using (Pen bottomPen = new Pen(Color.FromArgb(223, 220, 212)))
{
    e.Graphics.DrawLine(topPen, 0, 0, Width, 0);
    e.Graphics.DrawLine(bottomPen, 0, Height - 1, Width, Height - 1);
}
```

改为单线：

```csharp
using (Pen linePen = new Pen(Color.FromArgb(232, 233, 237)))
{
    e.Graphics.DrawLine(linePen, 0, Height - 1, Width, Height - 1);
}
```

- [ ] **Step 5: 新增非激活行鼠标悬停效果**

在类顶部字段区添加：

```csharp
private bool _isHovered;
```

在 `ApplyVisualState` 方法中，将背景色逻辑改为考虑 hover 状态。将：

```csharp
Color backColor;
if (isActive)
{
    backColor = UiPalette.ActivePaper;
}
else if (index % 2 == 0)
{
    backColor = UiPalette.PaperLight;
}
else
{
    backColor = UiPalette.PaperAlternate;
}
```

改为：

```csharp
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
```

在类中添加 `OnMouseEnter` 和 `OnMouseLeave` 重写：

```csharp
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
```

- [ ] **Step 6: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

- [ ] **Step 7: 提交**

```bash
git add src/ManuscriptCalculator/ManuscriptLineControl.cs
git commit -m "$(cat <<'EOF'
feat: 行控件视觉优化

行高 82→68，激活指示条改用蓝色 3px，分隔线双线改单线，新增非激活行悬停反馈。

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 4: 底部栏简化

**文件：**
- 修改：`src/ManuscriptCalculator/ManuscriptCalculatorForm.cs` — BuildFooter() 方法（第 284-311 行）

- [ ] **Step 1: 重写 BuildFooter() 方法**

将 `BuildFooter()` 方法替换为：

```csharp
private BufferedPanel BuildFooter()
{
    BufferedPanel footer = new BufferedPanel();
    footer.BackColor = Color.Transparent;
    footer.Height = 30;

    Label shortcuts = new Label();
    shortcuts.AutoSize = true;
    shortcuts.Text = "Enter 下一行    ↑↓ 切换行    Esc 收起";
    shortcuts.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
    shortcuts.ForeColor = UiPalette.InkSoft;

    footer.Controls.Add(shortcuts);

    footer.Resize += delegate
    {
        shortcuts.Location = new Point(4, 8);
    };

    return footer;
}
```

- [ ] **Step 2: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

- [ ] **Step 3: 提交**

```bash
git add src/ManuscriptCalculator/ManuscriptCalculatorForm.cs
git commit -m "$(cat <<'EOF'
feat: 简化底部栏，去掉左侧提示文案

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 5: 按钮微交互增强

**文件：**
- 修改：`src/ManuscriptCalculator/PillButton.cs`

WinForms 没有内置动画过渡。用一个 `System.Windows.Forms.Timer` 实现简单的 enter/leave 颜色渐变。

- [ ] **Step 1: 添加 Timer 驱动的颜色过渡**

将 `PillButton.cs` 完整替换为：

```csharp
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
        private Color _currentFill;
        private readonly Timer _animTimer;
        private int _animStep;
        private static readonly Color ScaleHover = Color.FromArgb(245, 246, 249);
        private static readonly Color ScalePress = Color.FromArgb(238, 239, 243);

        public PillButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            BackColor = UiPalette.CanvasTop;
            Cursor = Cursors.Hand;
            Size = new Size(88, 34);
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            _currentFill = FillColor;

            _animTimer = new Timer();
            _animTimer.Interval = 20;
            _animTimer.Tick += OnAnimTick;
        }

        public int CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; Invalidate(); }
        }

        public Color FillColor
        {
            get { return _fillColor; }
            set
            {
                _fillColor = value;
                if (!_isHovered && !_isPressed) _currentFill = value;
                Invalidate();
            }
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
            _animStep = 0;
            _animTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            _animStep = 0;
            _animTimer.Start();
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

        private void OnAnimTick(object sender, EventArgs e)
        {
            _animStep++;
            float t = Math.Min(1f, _animStep / 5f);
            t = t * t * (3f - 2f * t); // ease in-out

            if (_isPressed)
            {
                _currentFill = LerpColor(FillColor, PressedFillColor, 1f);
            }
            else if (_isHovered)
            {
                _currentFill = LerpColor(FillColor, HoverFillColor, t);
            }
            else
            {
                _currentFill = LerpColor(
                    _isPressed ? PressedFillColor : HoverFillColor,
                    FillColor,
                    t);
            }

            if (t >= 1f)
            {
                _animTimer.Stop();
            }

            Invalidate();
        }

        private static Color LerpColor(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
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

            Color fill = Enabled ? _currentFill : Color.FromArgb(235, 235, 235);
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
```

- [ ] **Step 2: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

- [ ] **Step 3: 提交**

```bash
git add src/ManuscriptCalculator/PillButton.cs
git commit -m "$(cat <<'EOF'
feat: 按钮增加颜色过渡动画和按压缩放反馈

Timer 驱动 ease-in-out 颜色渐变，press 时 0.97 缩放。

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 6: 复制结果闪烁反馈 + 窗口激活边框

**文件：**
- 修改：`src/ManuscriptCalculator/ManuscriptLineControl.cs` — 添加闪烁逻辑
- 修改：`src/ManuscriptCalculator/ManuscriptCalculatorForm.cs` — 窗口激活边框颜色

- [ ] **Step 1: 在 ManuscriptLineControl 中添加复制闪烁效果**

在 `ManuscriptLineControl` 类字段区添加：

```csharp
private readonly Timer _flashTimer;
private bool _flashOn;
private int _flashCount;
```

在构造函数中（`SetEvaluation(EvaluationState.Empty());` 之前）添加 Timer 初始化：

```csharp
_flashTimer = new Timer();
_flashTimer.Interval = 70;
_flashTimer.Tick += OnFlashTick;
```

在 `OnResultClicked` 方法末尾，复制到剪贴板后触发闪烁。替换 `OnResultClicked` 方法：

```csharp
private void OnResultClicked(object sender, EventArgs e)
{
    if (LastEvaluation != null && LastEvaluation.Success)
    {
        Clipboard.SetText(LastEvaluation.DisplayText);
        _copyToolTip.Hide(this);
        _copyToolTip.Show("已复制", this, _result.Left + _result.Width - 76, _result.Top - 8, 1100);

        // 触发蓝色闪烁
        _flashCount = 0;
        _flashOn = true;
        _flashTimer.Start();
    }
}
```

添加闪烁 Tick 处理方法：

```csharp
private void OnFlashTick(object sender, EventArgs e)
{
    _flashOn = !_flashOn;
    _flashCount++;

    if (_flashCount >= 6)
    {
        _flashTimer.Stop();
        _result.BackColor = BackColor;
    }
    else
    {
        _result.BackColor = _flashOn ? Color.FromArgb(220, 228, 255) : BackColor;
    }
}
```

- [ ] **Step 2: 在 ManuscriptCalculatorForm 中增加激活态边框颜色**

在 `OnPaint` 方法中（第 185-195 行），将边框绘制改为根据激活状态变色。添加一个字段：

```csharp
private bool _isActivated;
```

修改 `OnActivated` 和新增 `OnDeactivate`：

```csharp
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
```

修改 `OnPaint` 中的边框绘制，将：

```csharp
using (Pen pen = new Pen(UiPalette.WindowBorder))
{
    e.Graphics.DrawLine(pen, 0, 1, 0, Height - 2);
    e.Graphics.DrawLine(pen, Width - 1, 1, Width - 1, Height - 2);
    e.Graphics.DrawLine(pen, 1, Height - 1, Width - 2, Height - 1);
}
```

改为：

```csharp
Color borderColor = _isActivated ? UiPalette.AccentStart : UiPalette.WindowBorder;
using (Pen pen = new Pen(borderColor))
{
    e.Graphics.DrawLine(pen, 0, 1, 0, Height - 2);
    e.Graphics.DrawLine(pen, Width - 1, 1, Width - 1, Height - 2);
    e.Graphics.DrawLine(pen, 1, Height - 1, Width - 2, Height - 1);
}
```

- [ ] **Step 3: 构建验证**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

- [ ] **Step 4: 提交**

```bash
git add src/ManuscriptCalculator/ManuscriptLineControl.cs src/ManuscriptCalculator/ManuscriptCalculatorForm.cs
git commit -m "$(cat <<'EOF'
feat: 复制结果蓝色闪烁反馈 + 窗口激活边框变色

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
)"
```

---

### Task 7: 最终验证

- [ ] **Step 1: 完整构建**

```bash
cd E:\AI\softwares\win11_calculator
.\build.ps1
```

预期：编译零错误零警告，`dist\ManuscriptCalculator.exe` 生成成功。

- [ ] **Step 2: 运行并目视检查**

```bash
.\run.ps1
```

检查清单：
- [ ] 背景色为冷灰白渐变
- [ ] 标题栏扁平简洁，高度变窄
- [ ] 按钮有 hover 过渡动画
- [ ] 行高更紧凑，分隔线为单线
- [ ] 激活行左侧蓝色条
- [ ] 点击结果复制有蓝色闪烁
- [ ] 窗口边框激活时变蓝
- [ ] 底部栏只显示快捷键提示

- [ ] **Step 3: 推送**

```bash
git push
```
