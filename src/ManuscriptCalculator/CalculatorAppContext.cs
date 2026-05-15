using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Microsoft.Win32;

namespace ManuscriptCalculator
{
    internal sealed class CalculatorAppContext : IDisposable
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ManuscriptCalculator";

        private readonly MainWindow _window;
        private readonly DoubleCtrlMonitor _monitor;
        private readonly NotifyIcon _notifyIcon;
        private readonly Icon _appIcon;
        private readonly ToolStripMenuItem _showItem;
        private readonly ToolStripMenuItem _hideItem;
        private readonly ToolStripMenuItem _autoStartItem;
        private bool _isExiting;
        private IntPtr _handle;

        public CalculatorAppContext()
        {
            _appIcon = AppIconFactory.CreateCalculatorIcon();

            ContextMenuStrip menu = new ContextMenuStrip();
            _showItem = new ToolStripMenuItem("显示稿纸");
            _hideItem = new ToolStripMenuItem("收起到托盘");
            _autoStartItem = new ToolStripMenuItem("开机自启");
            _autoStartItem.CheckOnClick = true;
            _autoStartItem.Checked = IsAutoStartEnabled();
            _autoStartItem.Click += delegate { ToggleAutoStart(); };

            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出");

            _showItem.Click += delegate { ShowMainWindow(); };
            _hideItem.Click += delegate { HideMainWindow(); };
            exitItem.Click += delegate
            {
                _isExiting = true;
                System.Windows.Application.Current.Shutdown();
            };

            menu.Items.Add(_showItem);
            menu.Items.Add(_hideItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_autoStartItem);
            menu.Items.Add(exitItem);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = _appIcon;
            _notifyIcon.Text = "计算稿纸";
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += delegate { ShowMainWindow(); };

            var vm = new MainViewModel();
            vm.HideRequested += OnHideRequested;

            _window = new MainWindow();
            _window.DataContext = vm;
            _window.SourceInitialized += (s, e) =>
            {
                _handle = new WindowInteropHelper(_window).Handle;
            };
            _window.Closing += OnWindowClosing;
            _window.IsVisibleChanged += (s, e) => UpdateMenuState();

            _monitor = new DoubleCtrlMonitor();
            _monitor.Activated += delegate { ShowMainWindow(); };

            bool startMinimized = false;
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg.Equals("/minimized", StringComparison.OrdinalIgnoreCase))
                {
                    startMinimized = true;
                    break;
                }
            }

            if (!startMinimized)
            {
                ShowMainWindow();
                _notifyIcon.ShowBalloonTip(2500, "计算稿纸", "应用已驻留后台，双击 Alt 可再次唤起。", ToolTipIcon.Info);
            }
            else
            {
                UpdateMenuState();
            }
        }

        private bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    if (key == null) return false;
                    string value = key.GetValue(AppName) as string;
                    if (string.IsNullOrEmpty(value)) return false;
                    string exePath = Assembly.GetExecutingAssembly().Location;
                    return value.IndexOf(exePath, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch { return false; }
        }

        private void ToggleAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key == null) return;
                    if (_autoStartItem.Checked)
                    {
                        string exePath = Assembly.GetExecutingAssembly().Location;
                        key.SetValue(AppName, "\"" + exePath + "\" /minimized");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("设置开机自启失败：" + ex.Message, "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _autoStartItem.Checked = !_autoStartItem.Checked;
            }
        }

        private void OnHideRequested(object sender, EventArgs e)
        {
            HideMainWindow();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (_isExiting) return;
            e.Cancel = true;
            HideMainWindow();
        }

        private void ShowMainWindow()
        {
            if (_window.Visibility != Visibility.Visible)
            {
                _window.Show();
            }

            if (_window.WindowState == WindowState.Minimized)
            {
                _window.WindowState = WindowState.Normal;
            }

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.ShowAndActivate(_handle);
            }

            UpdateMenuState();
        }

        private void HideMainWindow()
        {
            if (_window.Visibility == Visibility.Visible)
            {
                _window.Hide();
            }

            UpdateMenuState();
        }

        private void UpdateMenuState()
        {
            bool visible = _window.Visibility == Visibility.Visible;
            _showItem.Enabled = !visible;
            _hideItem.Enabled = visible;
        }

        public void Dispose()
        {
            _isExiting = true;

            if (_monitor != null)
            {
                _monitor.Dispose();
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            if (_appIcon != null)
            {
                _appIcon.Dispose();
            }

            if (_window != null)
            {
                _window.Closing -= OnWindowClosing;
            }
        }
    }
}
