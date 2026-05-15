using System.Windows;

namespace ManuscriptCalculator
{
    public partial class App : Application
    {
        private CalculatorAppContext _appContext;

        protected override void OnStartup(StartupEventArgs e)
        {
            NativeMethods.EnableHighDpiMode();
            var themeService = new ThemeService();
            themeService.ApplyTheme();
            _appContext = new CalculatorAppContext();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_appContext != null)
            {
                _appContext.Dispose();
            }
            base.OnExit(e);
        }
    }
}
