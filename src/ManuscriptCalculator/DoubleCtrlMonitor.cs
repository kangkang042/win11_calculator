using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ManuscriptCalculator
{
    internal sealed class DoubleCtrlMonitor : IDisposable
    {
        private const int TapThresholdMilliseconds = 360;

        private readonly NativeMethods.LowLevelKeyboardProc _hookCallback;
        private IntPtr _hookHandle;
        private bool _altDown;
        private bool _otherKeyPressedWhileAltDown;
        private DateTime _lastTapTime = DateTime.MinValue;

        public event EventHandler Activated;

        public DoubleCtrlMonitor()
        {
            _hookCallback = HookProcedure;
            _hookHandle = InstallHook(_hookCallback);
        }

        public void Dispose()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }

        private static IntPtr InstallHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                IntPtr moduleHandle = NativeMethods.GetModuleHandle(module.ModuleName);
                return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc, moduleHandle, 0);
            }
        }

        private IntPtr HookProcedure(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                int message = wParam.ToInt32();
                NativeMethods.KbdLlHookStruct keyboardData = (NativeMethods.KbdLlHookStruct)Marshal.PtrToStructure(
                    lParam,
                    typeof(NativeMethods.KbdLlHookStruct));

                bool isAltKey = keyboardData.vkCode == NativeMethods.VK_MENU ||
                                keyboardData.vkCode == NativeMethods.VK_LMENU ||
                                keyboardData.vkCode == NativeMethods.VK_RMENU;

                if (isAltKey)
                {
                    if (message == NativeMethods.WM_KEYDOWN || message == NativeMethods.WM_SYSKEYDOWN)
                    {
                        if (!_altDown)
                        {
                            _altDown = true;
                            _otherKeyPressedWhileAltDown = false;
                        }
                    }
                    else if (message == NativeMethods.WM_KEYUP || message == NativeMethods.WM_SYSKEYUP)
                    {
                        if (_altDown)
                        {
                            _altDown = false;

                            if (!_otherKeyPressedWhileAltDown)
                            {
                                RegisterTap();
                            }
                        }
                    }
                }
                else if (_altDown && (message == NativeMethods.WM_KEYDOWN || message == NativeMethods.WM_SYSKEYDOWN))
                {
                    _otherKeyPressedWhileAltDown = true;
                }
            }

            return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
        }

        private void RegisterTap()
        {
            DateTime now = DateTime.UtcNow;
            if (_lastTapTime != DateTime.MinValue &&
                (now - _lastTapTime).TotalMilliseconds <= TapThresholdMilliseconds)
            {
                _lastTapTime = DateTime.MinValue;

                EventHandler handler = Activated;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }

                return;
            }

            _lastTapTime = now;
        }
    }
}
