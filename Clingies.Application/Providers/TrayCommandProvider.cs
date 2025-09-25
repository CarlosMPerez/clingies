using System.Windows.Input;
using Clingies.Application.Interfaces;


namespace Clingies.Application.Providers;

public class TrayCommandProvider : ITrayCommandProvider
{
    public ICommand NewCommand { get; }
    public ICommand RolledUpCommand { get; }
    public ICommand RolledDownCommand { get; }
    public ICommand PinnedCommand { get; }
    public ICommand UnpinnedCommand { get; }
    public ICommand LockedCommand { get; }
    public ICommand UnlockedCommand { get; }
    public ICommand ShowCommand { get; }
    public ICommand HideCommand { get; }
    public ICommand ManageClingiesCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand HelpCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand ExitCommand { get; }

    public TrayCommandProvider(ITrayCommandController controller)
    {
        NewCommand = new RelayCommand(controller.CreateNewClingy);
        RolledUpCommand = new RelayCommand(controller.RollUpAllClingies);
        RolledDownCommand = new RelayCommand(controller.RollDownAllClingies);
        PinnedCommand = new RelayCommand(controller.PinAllClingies);
        UnpinnedCommand = new RelayCommand(controller.UnpinAllClingies);
        LockedCommand = new RelayCommand(controller.LockAllClingies);
        UnlockedCommand = new RelayCommand(controller.UnlockAllClingies);
        ShowCommand = new RelayCommand(controller.ShowAllClingies);
        HideCommand = new RelayCommand(controller.HideAllClingies);
        ManageClingiesCommand = new RelayCommand(controller.ShowManageClingiesWindow);
        SettingsCommand = new RelayCommand(controller.ShowSettings);
        HelpCommand = new RelayCommand(controller.ShowHelpWindow);
        AboutCommand = new RelayCommand(controller.ShowAboutWindow);
        ExitCommand = new RelayCommand(controller.ExitApp);
    }
}
