using System;
using System.Windows.Media;
using Microsoft.Win32;

namespace ManuscriptCalculator
{
    public class ThemeService
    {
        public bool IsLightTheme { get; private set; }

        public ThemeService()
        {
            IsLightTheme = true;
            IsLightTheme = ReadSystemTheme();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        public void ApplyTheme()
        {
            IsLightTheme = ReadSystemTheme();
            var app = System.Windows.Application.Current;
            if (app == null) return;
            var dict = app.Resources.MergedDictionaries;
            dict.Clear();
            dict.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri(IsLightTheme ? "Styles/LightTheme.xaml" : "Styles/DarkTheme.xaml",
                                 UriKind.Relative)
            });
            dict.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri("Styles/Controls.xaml", UriKind.Relative)
            });
            ApplyAccentColor();
        }

        public void ApplyAccentColor()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;
            Color accent = ReadAccentColor();
            SolidColorBrush brush = new SolidColorBrush(accent);
            if (app.Resources.Contains("AccentBrush"))
                app.Resources["AccentBrush"] = brush;
            else
                app.Resources.Add("AccentBrush", brush);
        }

        private static Color ReadAccentColor()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AccentColor");
                        if (value is int)
                        {
                            int abgr = (int)value;
                            byte a = (byte)((abgr >> 24) & 0xFF);
                            byte b = (byte)((abgr >> 16) & 0xFF);
                            byte g = (byte)((abgr >> 8) & 0xFF);
                            byte r = (byte)(abgr & 0xFF);
                            if (a < 200) a = 255;
                            return Color.FromArgb(a, r, g, b);
                        }
                    }
                }
            }
            catch { }
            // 默认 Win11 蓝色
            return Color.FromRgb(0x4A, 0x6C, 0xF7);
        }

        private bool ReadSystemTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null && key.GetValue("AppsUseLightTheme") is int)
                    {
                        int intValue = (int)key.GetValue("AppsUseLightTheme");
                        return intValue != 0;
                    }
                }
            }
            catch { }
            return true;
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General)
            {
                bool current = ReadSystemTheme();
                if (current != IsLightTheme)
                {
                    var app = System.Windows.Application.Current;
                    if (app != null)
                    {
                        app.Dispatcher.Invoke(new Action(ApplyTheme));
                    }
                }
                else
                {
                    // 仅主题色变化
                    var app = System.Windows.Application.Current;
                    if (app != null)
                    {
                        app.Dispatcher.Invoke(new Action(ApplyAccentColor));
                    }
                }
            }
        }
    }
}
