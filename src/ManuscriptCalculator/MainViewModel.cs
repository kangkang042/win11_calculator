using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ManuscriptCalculator
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private LineViewModel _activeLine;

        public ObservableCollection<LineViewModel> Lines { get; private set; }

        public LineViewModel ActiveLine
        {
            get { return _activeLine; }
            set
            {
                if (_activeLine != null) _activeLine.IsActive = false;
                _activeLine = value;
                if (_activeLine != null) _activeLine.IsActive = true;
                OnPropertyChanged();
            }
        }

        public ICommand ClearAllCommand { get; private set; }
        public ICommand CopyActiveResultCommand { get; private set; }
        public ICommand HideCommand { get; private set; }

        public event EventHandler HideRequested;
        public event EventHandler LinesChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            Lines = new ObservableCollection<LineViewModel>();
            ClearAllCommand = new RelayCommand(_ => ClearAll());
            CopyActiveResultCommand = new RelayCommand(_ => CopyActiveResult());
            HideCommand = new RelayCommand(_ => { var h = HideRequested; if (h != null) h(this, EventArgs.Empty); });
            Lines.CollectionChanged += OnCollectionChanged;
            AddLine();
            ActiveLine = Lines[0];
            SyncRowIndices();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncRowIndices();
            var handler = LinesChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SyncRowIndices()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].RowIndex = i;
            }
        }

        public void AddLine()
        {
            var line = new LineViewModel();
            line.PropertyChanged += OnLinePropertyChanged;
            Lines.Add(line);
        }

        private void OnLinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Expression")
            {
                EnsureTrailingBlankLine();
            }
        }

        public void RemoveLine(LineViewModel line)
        {
            line.PropertyChanged -= OnLinePropertyChanged;
            Lines.Remove(line);
        }

        public void EnsureTrailingBlankLine()
        {
            if (Lines.Count == 0)
            {
                AddLine();
                return;
            }

            if (!string.IsNullOrWhiteSpace(Lines[Lines.Count - 1].Expression))
            {
                AddLine();
                return;
            }

            while (Lines.Count >= 2
                && string.IsNullOrWhiteSpace(Lines[Lines.Count - 1].Expression)
                && string.IsNullOrWhiteSpace(Lines[Lines.Count - 2].Expression)
                && Lines[Lines.Count - 1] != ActiveLine)
            {
                RemoveLine(Lines[Lines.Count - 1]);
            }
        }

        public void ClearAll()
        {
            while (Lines.Count > 0)
            {
                RemoveLine(Lines[Lines.Count - 1]);
            }
            AddLine();
            ActiveLine = Lines[0];
        }

        public void CopyActiveResult()
        {
            if (ActiveLine != null)
                ActiveLine.CopyResultCommand.Execute(null);
        }

        public void NavigateUp()
        {
            int index = Lines.IndexOf(ActiveLine);
            if (index > 0) ActiveLine = Lines[index - 1];
        }

        public void NavigateDown()
        {
            int index = Lines.IndexOf(ActiveLine);
            if (index >= 0 && index < Lines.Count - 1) ActiveLine = Lines[index + 1];
        }

        public void EnterPressed()
        {
            int index = Lines.IndexOf(ActiveLine);
            if (index < 0) return;
            EnsureTrailingBlankLine();
            if (index + 1 >= Lines.Count) AddLine();
            ActiveLine = Lines[Math.Min(index + 1, Lines.Count - 1)];
        }

        public void DeleteLineIfEmpty()
        {
            if (Lines.Count <= 1) return;
            if (string.IsNullOrWhiteSpace(ActiveLine != null ? ActiveLine.Expression : null))
            {
                int index = Lines.IndexOf(ActiveLine);
                if (index < 0) return;
                RemoveLine(ActiveLine);
                int nextIndex = Math.Max(0, index - 1);
                if (Lines.Count > 0) ActiveLine = Lines[nextIndex];
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
