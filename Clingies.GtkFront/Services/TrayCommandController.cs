using System;
using Clingies.Application.Interfaces;

namespace Clingies.GtkFront.Services;

public class TrayCommandController : ITrayCommandController
{
    private readonly ClingyWindowManager _windowFactory;
    private readonly IClingiesLogger _logger;

    public TrayCommandController(ClingyWindowManager windowFactory,
                                 IClingiesLogger logger)
    {
        _windowFactory = windowFactory;
        _logger = logger;
    }

    public void ShowSettings() => Console.WriteLine("SETTINGS NOT IMPLEMENTED");
    public void ExitApp() => Gtk.Application.Quit();
    public void RollUpAllClingies() => Console.WriteLine("ROLL UP ALL NOT IMPLEMENTED");
    public void RollDownAllClingies() => Console.WriteLine("ROLL DOWN ALL NOT IMPLEMENTED");
    public void PinAllClingies() => Console.WriteLine("PIN ALL NOT IMPLEMENTED");
    public void UnpinAllClingies() => Console.WriteLine("UNPIN ALL NOT IMPLEMENTED");
    public void LockAllClingies() => Console.WriteLine("LOCK ALL NOT IMPLEMENTED");
    public void UnlockAllClingies() => Console.WriteLine("UNLOCK ALL NOT IMPLEMENTED");
    public void ShowAllClingies() => _windowFactory.RenderAllWindows();
    public void HideAllClingies() => Console.WriteLine("HIDE ALL NOT IMPLEMENTED");
    public void ShowManageClingiesWindow() => Console.WriteLine("MANAGE CLINGIES NOT IMPLEMENTED");
    public void ShowHelpWindow() => Console.WriteLine("HELP WINDOW NOT IMPLEMENTED");
    public void ShowAboutWindow() => Console.WriteLine("ABOUT WINDOW NOT IMPLEMENTED");

    public void CreateNewClingy() => _windowFactory.CreateNewWindow();

    public void ShowStyleManager() => _windowFactory.ShowStyleManagerDialog();
}
