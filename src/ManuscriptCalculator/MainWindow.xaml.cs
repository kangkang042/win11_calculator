using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ManuscriptCalculator
{
    public partial class MainWindow : Window
    {
        private MainViewModel VM
        {
            get { return (MainViewModel)DataContext; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnSourceInitialized(object sender, System.EventArgs e)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            NativeMethods.ApplyWindowChrome(helper.Handle);
        }

        private void OnActivated(object sender, System.EventArgs e)
        {
            InvalidateVisual();
        }

        private void OnDeactivated(object sender, System.EventArgs e)
        {
            InvalidateVisual();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) DragMove();
        }

        private void OnCopyClicked(object sender, RoutedEventArgs e)
        {
            var vm = VM;
            if (vm != null) vm.CopyActiveResult();
        }

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            var vm = VM;
            if (vm != null) vm.ClearAllCommand.Execute(null);
        }

        private void OnHideClicked(object sender, RoutedEventArgs e)
        {
            var vm = VM;
            if (vm != null) vm.HideCommand.Execute(null);
        }

        private void OnRowClicked(object sender, MouseButtonEventArgs e)
        {
            var row = sender as CalculatorLine;
            if (row != null)
            {
                var line = row.DataContext as LineViewModel;
                if (line != null)
                {
                    VM.ActiveLine = line;
                }
            }
        }
    }
}
