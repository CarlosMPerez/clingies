using System.Windows.Input;

namespace Clingies.Application.Interfaces;

public interface IContextCommandProvider
{
    ICommand SleepCommand { get; }
    ICommand ShowAlarmWindowCommand { get; }
    ICommand ShowChangeTitleDialogCommand { get; }
    ICommand ShowColorWindowCommand { get; }
    ICommand LockCommand { get; }
    ICommand UnlockCommand { get; }
    ICommand RollUpCommand { get; }
    ICommand RollDownCommand { get; }
    ICommand ShowPropertiesWindowCommand { get; }
    ICommand ShowStyleManagerCommand { get; }
}
