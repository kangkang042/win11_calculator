# 计算稿纸

这是一个原生 Windows 桌面计算应用，特性如下：

- 双击 `Alt` 呼出主窗口
- 逐行输入公式，右侧实时显示结果
- 左键单击右侧结果，直接复制结果
- 关闭窗口时自动收起到系统托盘，热键继续可用

## 运行方式

在当前目录执行：

```powershell
.\run.ps1
```

只构建不启动：

```powershell
.\build.ps1
```

生成结果：

- `dist\ManuscriptCalculator.exe`

## 使用说明

- 双击 `Alt`：显示并聚焦窗口
- 左键单击右侧结果：直接复制结果
- `Enter`：跳到下一行
- `Up` / `Down`：切换行
- `Backspace`：在空白行删除当前行
- `Esc`：收起到托盘
- 托盘图标右键可退出程序

## 表达式支持

- 基本四则运算：`+ - * /`
- 括号：`( )`
- 百分比：`50%`
- 幂运算：`2^8`
- 常量：`pi`、`e`
- 函数：`sqrt`、`abs`、`sin`、`cos`、`tan`、`log`、`ln`、`round`、`floor`、`ceil`、`min`、`max`、`pow`
