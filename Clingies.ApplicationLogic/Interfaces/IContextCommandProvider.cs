using System.Windows.Input;

namespace Clingies.ApplicationLogic.Interfaces;

public interface IContextCommandProvider
{
    ICommand SleepCommand { get; }
    ICommand AttachCommand { get; }
    ICommand BuildStackMenuCommand { get; }
    ICommand ShowAlarmWindowCommand { get; }
    ICommand ShowChangeTitleDialogCommand { get; }
    ICommand ShowColorWindowCommand { get; }
    ICommand LockCommand { get; }
    ICommand UnlockCommand { get; }
    ICommand ShowPropertiesWindowCommand { get; }
}
