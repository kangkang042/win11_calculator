using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ManuscriptCalculator
{
    public class LineViewModel : INotifyPropertyChanged
    {
        private string _expression = string.Empty;
        private string _displayResult = "=";
        private bool _hasError;
        private bool _isActive;
        private bool _isHovered;
        private System.Windows.Threading.DispatcherTimer _copyTimer;
        private bool _showCopyTip;

        public bool ShowCopyTip
        {
            get { return _showCopyTip; }
            set { _showCopyTip = value; OnPropertyChanged(); }
        }

        public string Expression
        {
            get { return _expression; }
            set
            {
                if (_expression != value)
                {
                    _expression = value;
                    OnPropertyChanged();
                    OnPropertyChanged("HasExpression");
                    Evaluate();
                }
            }
        }

        public string DisplayResult
        {
            get { return _displayResult; }
            set { _displayResult = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; OnPropertyChanged(); }
        }

        public bool HasExpression
        {
            get { return !string.IsNullOrWhiteSpace(_expression); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged();
                OnPropertyChanged("ActiveBackground");
            }
        }

        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                _isHovered = value;
                OnPropertyChanged();
                OnPropertyChanged("ActiveBackground");
            }
        }

        private int _rowIndex;
        public int RowIndex
        {
            get { return _rowIndex; }
            set { _rowIndex = value; OnPropertyChanged(); }
        }

        public EvaluationState LastEvaluation { get; private set; }

        public ICommand CopyResultCommand { get; private set; }

        public LineViewModel()
        {
            LastEvaluation = EvaluationState.Empty();
            CopyResultCommand = new RelayCommand(_ => CopyResult());
        }

        private void Evaluate()
        {
            LastEvaluation = ExpressionEvaluator.Evaluate(_expression);
            if (!LastEvaluation.HasExpression)
            {
                DisplayResult = "=";
                HasError = false;
            }
            else if (LastEvaluation.Success)
            {
                DisplayResult = "= " + LastEvaluation.DisplayText;
                HasError = false;
            }
            else
            {
                DisplayResult = LastEvaluation.ErrorMessage;
                HasError = true;
            }
        }

        private void CopyResult()
        {
            if (!LastEvaluation.Success) return;
            Clipboard.SetText(LastEvaluation.DisplayText);

            if (_copyTimer == null)
            {
                _copyTimer = new System.Windows.Threading.DispatcherTimer();
                _copyTimer.Interval = TimeSpan.FromSeconds(1.2);
                _copyTimer.Tick += (s, e) =>
                {
                    _copyTimer.Stop();
                    ShowCopyTip = false;
                };
            }
            else
            {
                _copyTimer.Stop();
            }

            ShowCopyTip = true;
            _copyTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
