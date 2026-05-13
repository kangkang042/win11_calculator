# 计算稿纸界面优化设计

日期：2026-05-13

## 目标

在不变更布局结构的前提下，升级视觉风格（简洁现代）和交互体验（过渡动画、微反馈、键盘引导）。

## 方案：配色重塑 + 标题栏简化 + 行控件优化 + 微交互增强

### 一、配色方案

从暖色调切换为中性冷色调：

| 常量 | 现行值 | 新值 | 说明 |
|------|--------|------|------|
| CanvasTop | `#FFFFFF` | `#F8F9FB` | 背景渐变上 |
| CanvasBottom | `#FCFCFC` | `#F2F3F6` | 背景渐变下 |
| WindowBorder | `#EBECF0` | `#E5E6EA` | 窗口边框 |
| AccentStart | `#FF9C18` | `#4A6CF7` | 强调色主色 |
| AccentEnd | `#FFBC52` | `#6B8AFF` | 强调色辅色 |
| InkStrong | `#16314E` | `#1A1D26` | 主文字色 |
| InkMuted | `#6E7985` | `#888C96` | 辅助文字色 |
| InkSoft | `#969DA5` | `#B0B4BC` | 占位文字色 |
| PaperLight | `#FFFFFF` | `#FFFFFF` | 行背景（偶数） |
| PaperAlternate | `#FEFEFE` | `#FAFBFC` | 行背景（奇数） |
| ActivePaper | `#FCF8EE` | `#EEF1FA` | 激活行背景 |
| Error | `#C1543F` | `#D1524F` | 错误色 |

文件：`UiPalette.cs`，只改色值不改结构。

### 二、标题栏简化

文件：`ManuscriptCalculatorForm.cs` BuildTitleBar()

- 品牌标识：移除 SurfacePanel 圆角背景，图标+标题纯文字排列
- 字体：标题 13.5pt→11pt，去掉副标题"逐行演算"
- 标题栏高度：80→56
- 按钮：CornerRadius 从 17 降为 10，字体减小
- 整行保持拖拽功能

### 三、行控件优化

文件：`ManuscriptLineControl.cs`

- 激活左侧条：4px→3px，颜色改用 AccentStart（蓝色）
- 行间分隔线：双线→单线（`#E8E9ED`）
- 行高：82→68
- 交替色：差异更微弱
- 新增悬停效果：非激活行 hover 背景微变（`#F7F8FB`）

### 四、底部栏简化

文件：`ManuscriptCalculatorForm.cs` BuildFooter()

- 去掉左侧"关闭窗口会收起到托盘"
- 右侧改为更清晰的快捷键提示

### 五、微交互增强

| 交互 | 改动 |
|------|------|
| 按钮 hover/press | 增加过渡动画、轻微缩放 |
| 复制结果 | 结果区短暂蓝色闪烁（Timer 驱动，200ms） |
| 滚动平滑 | 行间滚动时平滑过渡 |
| 窗口激活边框 | 激活时边框变蓝色强调 |

涉及文件：`PillButton.cs`、`ManuscriptLineControl.cs`

### 不涉及的文件

- `BufferedPanel.cs` — 无变化
- `UiHelpers.cs` — 无变化
- `SurfacePanel.cs` — 标题栏简化后可能不再使用，按需保留
- `CalculatorGlyphBadge.cs` — 保持现有绘制
- `ExpressionEvaluator.cs`、`EvaluationState.cs` — 无变化
- `CalculatorAppContext.cs`、`DoubleCtrlMonitor.cs`、`NativeMethods.cs` — 无变化
