
namespace Clingies.Application.Interfaces;

public interface ITrayCommandController
{
    void CreateNewClingy();
    void RollUpAllClingies();
    void RollDownAllClingies();
    void PinAllClingies();
    void UnpinAllClingies();
    void LockAllClingies();
    void UnlockAllClingies();
    void ShowAllClingies();
    void HideAllClingies();
    void ShowManageClingiesWindow();
    void ShowSettings();
    void ShowHelpWindow();
    void ShowAboutWindow();
    void ExitApp();
}
