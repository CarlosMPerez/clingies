using System.Windows.Input;

namespace Clingies.Application.Providers;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter)
    {
        _execute();
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : ICommand
{
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T v) return _canExecute?.Invoke(v) ?? true;
            if (parameter is string s && typeof(T) == typeof(int) && int.TryParse(s, out var i))
                return _canExecute?.Invoke((T)(object)i) ?? true;
            return _canExecute?.Invoke(default!) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T v) { _execute(v); return; }
            if (parameter is string s && typeof(T) == typeof(int) && int.TryParse(s, out var i))
            { _execute((T)(object)i); return; }
            throw new ArgumentException($"Invalid parameter for RelayCommand<{typeof(T).Name}>");
        }

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);    
}