using System.Windows.Input;
using Clingies.Application.Interfaces;

namespace Clingies.Application.Providers;

public class ContextCommandProvider : IContextCommandProvider
{
    private IContextCommandController _controller;
    public ICommand SleepCommand { get; }
    //public ICommand BuildStackMenuCommand { get; }
    public ICommand ShowAlarmWindowCommand { get; }
    public ICommand ShowChangeTitleDialogCommand { get; }
    public ICommand ShowColorWindowCommand { get; }
    public ICommand LockCommand { get; }
    public ICommand UnlockCommand { get; }
    public ICommand RollUpCommand { get; }
    public ICommand RollDownCommand { get; }
    public ICommand ShowPropertiesWindowCommand { get; }

    public ContextCommandProvider(IContextCommandController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        SleepCommand = new RelayCommand(_controller.SleepClingy);
        ShowAlarmWindowCommand = new RelayCommand(_controller.ShowAlarmWindow);
        ShowChangeTitleDialogCommand = new RelayCommand(_controller.ShowChangeTitleDialog);
        ShowColorWindowCommand = new RelayCommand(_controller.ShowColorWindow);
        LockCommand = new RelayCommand(_controller.LockClingy);
        UnlockCommand = new RelayCommand(_controller.UnlockClingy);
        RollUpCommand = new RelayCommand(_controller.RollUpClingy);
        RollDownCommand = new RelayCommand(_controller.RollDownClingy);
        ShowPropertiesWindowCommand = new RelayCommand(_controller.ShowPropertiesWindow);
    }
}
