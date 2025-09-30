namespace Clingies.Application.Interfaces;

public interface IContextCommandController
{
    void SleepClingy();
    void ShowAlarmWindow();
    void ShowChangeTitleDialog();
    void ShowColorWindow();
    void LockClingy();
    void UnlockClingy();
    void RollUpClingy();
    void RollDownClingy();
    void ShowPropertiesWindow();
    void ApplyStyle(int styleId);
}
