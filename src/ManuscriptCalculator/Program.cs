using System;
using System.Windows.Forms;

namespace ManuscriptCalculator
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            NativeMethods.EnableHighDpiMode();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CalculatorAppContext());
        }
    }
}
