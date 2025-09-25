using System.Windows.Input;

namespace Clingies.Application.Interfaces;

public interface ITrayCommandProvider
{
    ICommand NewCommand { get; }
    ICommand RolledUpCommand { get; }
    ICommand RolledDownCommand { get; }
    ICommand PinnedCommand { get; }
    ICommand UnpinnedCommand { get; }
    ICommand LockedCommand { get; }
    ICommand UnlockedCommand { get; }
    ICommand ShowCommand { get; }
    ICommand HideCommand { get; }
    ICommand ManageClingiesCommand { get; }
    ICommand SettingsCommand { get; }
    ICommand HelpCommand { get; }
    ICommand AboutCommand { get; }
    ICommand ExitCommand { get; }
}
