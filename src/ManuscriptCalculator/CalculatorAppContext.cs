using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ManuscriptCalculator
{
    internal sealed class CalculatorAppContext : ApplicationContext
    {
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ManuscriptCalculator";

        private readonly ManuscriptCalculatorForm _form;
        private readonly DoubleCtrlMonitor _monitor;
        private readonly NotifyIcon _notifyIcon;
        private readonly Icon _appIcon;
        private readonly ToolStripMenuItem _showItem;
        private readonly ToolStripMenuItem _hideItem;
        private readonly ToolStripMenuItem _autoStartItem;
        private bool _isExiting;

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
                ExitThread();
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

            _form = new ManuscriptCalculatorForm();
            _form.Icon = _appIcon;
            _form.HideRequested += OnHideRequested;
            _form.FormClosing += OnFormClosing;
            _form.VisibleChanged += delegate { UpdateMenuState(); };

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
                    return value.IndexOf(Application.ExecutablePath, StringComparison.OrdinalIgnoreCase) >= 0;
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
                        key.SetValue(AppName, "\"" + Application.ExecutablePath + "\" /minimized");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置开机自启失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _autoStartItem.Checked = !_autoStartItem.Checked;
            }
        }

        private void OnHideRequested(object sender, EventArgs e)
        {
            HideMainWindow();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isExiting)
            {
                return;
            }

            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideMainWindow();
            }
        }

        private void ShowMainWindow()
        {
            if (!_form.Visible)
            {
                _form.Show();
            }

            if (_form.WindowState == FormWindowState.Minimized)
            {
                _form.WindowState = FormWindowState.Normal;
            }

            NativeMethods.ShowAndActivate(_form.Handle);
            _form.FocusActiveEditor();
            UpdateMenuState();
        }

        private void HideMainWindow()
        {
            if (_form.Visible)
            {
                _form.Hide();
            }

            UpdateMenuState();
        }

        private void UpdateMenuState()
        {
            bool visible = _form.Visible;
            _showItem.Enabled = !visible;
            _hideItem.Enabled = visible;
        }

        protected override void ExitThreadCore()
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

            if (_form != null)
            {
                _form.Dispose();
            }

            if (_appIcon != null)
            {
                _appIcon.Dispose();
            }

            base.ExitThreadCore();
        }
    }
}
