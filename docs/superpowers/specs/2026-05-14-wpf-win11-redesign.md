# 计算稿纸 WPF + Win11 原生风格重设计

> **目标**：将 WinForms 计算稿纸迁移到 WPF，实现 Windows 11 原生视觉质感——Mica 云母背景、WinUI 控件风格、流畅动画、系统主题跟随。

**技术栈**：.NET Framework 4.8 + WPF（XAML + MVVM）

**架构**：单项目 MVVM，计算引擎（ExpressionEvaluator/EvaluationState）原样复用

---

## 架构

```
src/ManuscriptCalculator/
├── App.xaml / App.xaml.cs          # 应用入口、主题加载
├── MainWindow.xaml / .cs           # 主窗口（Mica 背景、窗口 Chrome）
├── Models/
│   ├── ExpressionEvaluator.cs      # ← 复用
│   ├── EvaluationState.cs          # ← 复用
│   └── LineModel.cs                # 新：行数据模型
├── ViewModels/
│   ├── MainViewModel.cs            # 主 VM
│   └── LineViewModel.cs            # 行 VM
├── Views/
│   ├── CalculatorLine.xaml / .cs   # 行控件
│   └── PillButton.xaml / .cs       # 按钮控件（自定义控件）
├── Styles/
│   ├── LightTheme.xaml             # 亮色主题资源
│   ├── DarkTheme.xaml              # 暗色主题资源
│   └── Controls.xaml               # 控件模板
├── Converters/
│   └── ...                         # Bool/String/Color 转换器
├── Services/
│   └── ThemeService.cs             # 系统主题监听
├── UiPalette.cs                    # ← 重构为适配双主题
├── NativeMethods.cs                # ← 复用（DWM/WndProc）
├── AppIconFactory.cs               # ← 复用
├── CalculatorAppContext.cs         # ← 复用（托盘逻辑）
├── DoubleCtrlMonitor.cs            # ← 复用（快捷键监听）
└── Program.cs                      # ← 修改：启动 WPF 应用
```

### MVVM 数据流

```
MainViewModel
├── ObservableCollection<LineViewModel> Lines
├── ICommand ClearAllCommand
├── ICommand HideCommand
├── LineViewModel ActiveLine
└── bool IsActivated

LineViewModel
├── string ExpressionText (双向绑定到 TextBox)
├── string DisplayResult (只读，计算后更新)
├── bool IsActive
├── bool HasError
├── string ErrorMessage
└── ICommand CopyResultCommand
```

### 窗口架构

```
MainWindow (WindowStyle=None, AllowsTransparency=True)
├── Grid (Mica 穿透背景)
│   ├── 标题栏 (自定义拖拽区 + 图标 + 标题 + 清空/收起按钮)
│   ├── ItemsControl + ScrollViewer (行列表)
│   │   └── CalculatorLine × N
│   └── Footer 状态栏 (快捷键提示)
```

---

## Mica 云母背景

### 实现

1. `WindowChrome` 移除标准标题栏，`ResizeMode=CanResizeWithGrip`
2. 窗口背景设为 `Transparent`，Mica 从 DWM 层透出
3. `SourceInitialized` 事件中 P/Invoke `DwmSetWindowAttribute`，设置 `DWMWA_SYSTEMBACKDROP_TYPE = 2 (Mica)`
4. 标题栏用自定义 `Grid` + `MouseLeftButtonDown` 实现拖拽

### 回退策略

| 环境 | 方案 |
|------|------|
| Windows 11 22H2+ | Mica |
| Windows 10 | 亚克力近似（DwmExtendFrameIntoClientArea） |
| 旧版/虚拟机 | 纯色 `#F5F6F8` |

---

## WinUI 控件风格

### 行控件 CalculatorLine

| 属性 | 值 |
|------|-----|
| 外观 | 圆角 8px 卡片 (`Border.CornerRadius="8"`) |
| 偶数行背景 | `PaperLight` |
| 奇数行背景 | `PaperAlternate` |
| 激活行背景 | `ActivePaper`（亮 `#EEF1FA` / 暗 `#2A2D3E`） |
| 激活指示 | 仅背景色区分，无左侧竖条 |
| 输入区 | 透明背景 `TextBox`，`Cascadia Mono 19pt`，无边框 |
| 占位符 | 浅色文字"输入公式" |
| 结果区 | `TextBlock`，`Cascadia Mono 20pt Bold`，右对齐 |
| 结果可点击复制 | Cursor=Hand，ToolTip"已复制" |
| 行间分隔 | `BorderThickness="0,0,0,1"` + `SeparatorColor` |
| 行高 | 78px |
| hover 反馈 | 背景微变（亮 `#F7F8FB` / 暗 `#2F2F2F`） |

### 按钮 PillButton

| 属性 | 值 |
|------|-----|
| 圆角 | 18px |
| 最小宽度 | 64px |
| 高度 | 48px |
| 字体 | `Microsoft YaHei UI 12pt Bold` |
| 默认态 | 白底 + 浅灰边框 (`FillColor` / `BorderColor`) |
| Hover | 浅蓝灰填充 (`HoverFillColor`) 100ms 渐变 |
| Press | 缩放 0.97 即时 + 100ms 弹回 |
| 禁用态 | 灰色文字 + 浅灰填充 |
| Cursor | Hand |

### 窗口边框

| 状态 | 颜色 |
|------|------|
| 激活 | 系统强调色 1px |
| 非激活 | `WindowBorder` 1px |

### 排版层级

| 角色 | 字体 | 大小 | 字重 |
|------|------|------|------|
| 标题 | Microsoft YaHei UI | 14pt | Bold |
| 公式输入 | Cascadia Mono | 19pt | Regular |
| 计算结果 | Cascadia Mono | 20pt | Bold |
| 占位符 | Microsoft YaHei UI | 14pt | Regular |
| 按钮文字 | Microsoft YaHei UI | 12pt | Bold |
| 底部提示 | Microsoft YaHei UI | 9pt | Regular |

### 系统强调色

- 启动时从 `DwmGetColorizationColor` 读取系统强调色
- 替换 `AccentStart`，亮/暗模式下自动调整饱和度
- 回退值：亮色 `#4A6CF7`，暗色 `#6B8AFF`

---

## 流畅动画

全部使用 WPF `Storyboard` + `DoubleAnimation` / `ColorAnimation`，无 Timer。

### 进场动画

| 效果 | 时长 | 缓动 |
|------|------|------|
| 窗口透明度 0→1 | 200ms | ease-out-quart |

### 行操作

| 触发 | 动画 | 时长 | 缓动 |
|------|------|------|------|
| 新行插入 | 高度 0→78px + 透明度 0→1 | 200ms | ease-out-quart |
| 行删除 | 高度 78→0px + 透明度 1→0 | 150ms | ease-in-quart |
| 结果更新 | 缩放 1.0→1.05→1.0（仅结果数字） | 150ms | ease-out-quint |

### 按钮

| 状态 | 效果 | 时长 | 缓动 |
|------|------|------|------|
| Hover 入 | 背景色渐变 | 100ms | — |
| Hover 出 | 背景色渐变 | 100ms | — |
| Press | 缩放 0.97 | 即时 | — |
| Release | 缩放复原 | 100ms | ease-out |

### 焦点切换

- 激活行背景色渐变 | 150ms | ease-out

### 降级

- 系统设置"减少动画"时跳过所有动画，直接切状态

---

## 系统主题跟随

### 监听机制

- `ThemeService` 监听 `Microsoft.Win32.SystemEvents.UserPreferenceChanged`
- 注册表读取 `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme`
- 启动时读取一次，系统切换时实时更新

### 双主题色表

| 色板角色 | 亮色模式 | 暗色模式 |
|----------|----------|----------|
| `Canvas`（窗口背景） | `#F5F6F8` | `#1F1F1F` |
| `PaperLight`（偶数行） | `#FCFCFD` | `#2D2D2D` |
| `PaperAlternate`（奇数行） | `#FAFBFC` | `#282828` |
| `ActivePaper`（激活行） | `#EEF1FA` | `#2A2D3E` |
| `InkStrong`（主文字） | `#1A1D26` | `#E8E8E8` |
| `InkMuted`（次要文字） | `#888C96` | `#9A9A9A` |
| `InkSoft`（占位/提示） | `#B0B4BC` | `#6A6A6A` |
| `WindowBorder` | `#E5E6EA` | `#3A3A3A` |
| `AccentStart`（强调色） | 系统值 | 系统值 |
| `Error` | `#D1524F` | `#FF6B6B` |
| `SeparatorColor`（行分割线） | `#E8E9ED` | `#333333` |
| `ButtonHoverFill` | `#EEF0F6` | `#3A3A3A` |

### 实现

- `LightTheme.xaml` / `DarkTheme.xaml` 各定义完整 `ResourceDictionary`
- `App.xaml` 启动时按系统主题加载
- `DynamicResource` 标记所有颜色引用，切换主题时自动刷新

---

## 功能保留清单

以下 WinForms 版功能原样保留，不增不减：

- [x] 逐行输入公式，右侧实时显示结果
- [x] 左键单击右侧结果，直接复制结果
- [x] 双击 `Alt` 呼出主窗口
- [x] 关闭窗口时自动收起到系统托盘
- [x] `Enter` 跳到下一行
- [x] `Up` / `Down` 切换行
- [x] `Backspace` 在空白行删除当前行
- [x] `Ctrl+C` 复制当前行结果
- [x] `Ctrl+R` 清空全部公式
- [x] `Esc` 收起到托盘
- [x] 全角字符自动转半角（Normalize）
- [x] 托盘图标右键退出程序
