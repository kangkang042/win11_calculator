using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ManuscriptCalculator
{
    public partial class CalculatorLine : UserControl
    {
        public CalculatorLine()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            MouseEnter += (s, e) =>
            {
                var vm = DataContext as LineViewModel;
                if (vm != null) vm.IsHovered = true;
            };
            MouseLeave += (s, e) =>
            {
                var vm = DataContext as LineViewModel;
                if (vm != null) vm.IsHovered = false;
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as LineViewModel;
            if (vm != null)
            {
                UpdateBackground();
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == "IsActive"
                        || args.PropertyName == "IsHovered"
                        || args.PropertyName == "RowIndex")
                    {
                        UpdateBackground();
                    }
                    if (args.PropertyName == "IsActive" && vm.IsActive)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Input,
                            new Action(() =>
                            {
                                editor.Focus();
                                editor.CaretIndex = editor.Text.Length;
                            }));
                    }
                };
            }
        }

        private void UpdateBackground()
        {
            var vm = DataContext as LineViewModel;
            if (vm == null) return;
            string key;
            if (vm.IsActive)
                key = "ActivePaperBrush";
            else if (vm.IsHovered)
                key = "HoverRowBrush";
            else if (vm.RowIndex % 2 == 0)
                key = "PaperLightBrush";
            else
                key = "PaperAlternateBrush";
            root.Background = (System.Windows.Media.Brush)FindResource(key);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            var window = Window.GetWindow(this);
            var vm = window != null ? window.DataContext as MainViewModel : null;
            if (vm == null) return;

            switch (e.Key)
            {
                case Key.Enter:
                    vm.EnterPressed();
                    e.Handled = true;
                    break;
                case Key.Up:
                    vm.NavigateUp();
                    e.Handled = true;
                    break;
                case Key.Down:
                    vm.NavigateDown();
                    e.Handled = true;
                    break;
                case Key.Back:
                    var tb = sender as TextBox;
                    if (tb != null && string.IsNullOrEmpty(tb.Text))
                    {
                        vm.DeleteLineIfEmpty();
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void OnResultClicked(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as LineViewModel;
            if (vm != null) vm.CopyResultCommand.Execute(null);
        }
    }
}
