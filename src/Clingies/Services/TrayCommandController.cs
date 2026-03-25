using System;

namespace Clingies.Services;

public class TrayCommandController(ClingyWindowManager windowFactory)
{
    public void ShowSettings() => Console.WriteLine("SETTINGS NOT IMPLEMENTED");
    public void ExitApp() => Gtk.Application.Quit();
    public void RollUpAllClingies() => Console.WriteLine("ROLL UP ALL NOT IMPLEMENTED");
    public void RollDownAllClingies() => Console.WriteLine("ROLL DOWN ALL NOT IMPLEMENTED");
    public void PinAllClingies() => Console.WriteLine("PIN ALL NOT IMPLEMENTED");
    public void UnpinAllClingies() => Console.WriteLine("UNPIN ALL NOT IMPLEMENTED");
    public void LockAllClingies() => Console.WriteLine("LOCK ALL NOT IMPLEMENTED");
    public void UnlockAllClingies() => Console.WriteLine("UNLOCK ALL NOT IMPLEMENTED");
    public void ShowAllClingies() => windowFactory.RenderAllWindows();
    public void HideAllClingies() => Console.WriteLine("HIDE ALL NOT IMPLEMENTED");
    public void ShowManageClingiesWindow() => Console.WriteLine("MANAGE CLINGIES NOT IMPLEMENTED");
    public void ShowHelpWindow() => Console.WriteLine("HELP WINDOW NOT IMPLEMENTED");
    public void ShowAboutWindow() => Console.WriteLine("ABOUT WINDOW NOT IMPLEMENTED");

    public void CreateNewClingy() => windowFactory.CreateNewWindow();

    public void ShowStyleManager() => windowFactory.ShowStyleManagerDialog();
}
