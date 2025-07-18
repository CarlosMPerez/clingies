using System.Windows.Input;

namespace Clingies.ApplicationLogic.Interfaces;

public interface ITrayCommandProvider
{
    ICommand NewCommand { get; }
    ICommand SettingsCommand { get; }
    ICommand ExitCommand { get; }
}
