using System.Windows.Input;
using Clingies.ApplicationLogic.Interfaces;

namespace Clingies.ApplicationLogic.Providers;

public class ContextCommandProvider : IContextCommandProvider
{
    private IContextCommandController _controller;
    public ICommand SleepCommand { get; }
    public ICommand BuildStackMenuCommand { get; }
    public ICommand ShowAlarmWindowCommand { get; }
    public ICommand ShowChangeTitleDialogCommand { get; }
    public ICommand ShowColorWindowCommand { get; }
    public ICommand LockCommand { get; }
    public ICommand UnlockCommand { get; }
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
        ShowPropertiesWindowCommand = new RelayCommand(_controller.ShowPropertiesWindow);
    }
}
