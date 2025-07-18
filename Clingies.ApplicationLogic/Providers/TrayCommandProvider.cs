using System.Windows.Input;
using Clingies.ApplicationLogic.Interfaces;


namespace Clingies.ApplicationLogic.Providers;

public class TrayCommandProvider : ITrayCommandProvider
{
    public ICommand NewCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand ExitCommand { get; }

    public TrayCommandProvider(IClingiesCommandController controller)
    {
        NewCommand = new RelayCommand(controller.CreateNewClingy);
        SettingsCommand = new RelayCommand(controller.ShowSettings);
        ExitCommand = new RelayCommand(controller.ExitApp);
    }
}
