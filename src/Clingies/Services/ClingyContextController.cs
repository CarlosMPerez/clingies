using System;

namespace Clingies.Services;

public class ClingyContextController(ClingyWindowManager manager,
                                ClingyStylingService stylingService,
                                ITitleDialogService titleDialog,
                                int clingyId)
{
    public void ShowAlarmWindow() => Console.WriteLine("ALARM TO BE IMPLEMENTED");

    public void ShowChangeTitleDialog()
    {
        var prevTitle = manager.GetClingyModelById(clingyId)?.Title;
        var owner = manager.GetClingyWindowById(clingyId);
        var newTitle = titleDialog.ShowChangeTitleDialog(owner, prevTitle);
        if (newTitle == null) return;
        manager.RequestTitleChange(clingyId, newTitle);
    }

    public void ApplyStyle(int styleId)
    {
        var owner = manager.GetClingyWindowById(clingyId);
        stylingService.ApplyTo(owner!, clingyId, styleId);
        manager.RequestStyleChange(clingyId, styleId);
    }

    public void ShowColorWindow() => Console.WriteLine("COLOR TO BE IMPLEMENTED");

    public void ShowPropertiesWindow() => Console.WriteLine("PROPERTIES TO BE IMPLEMENTED");

    public void SleepClingy() => Console.WriteLine("SLEEP TO BE IMPLEMENTED");

    public void LockClingy() => manager.RequestLock(clingyId);

    public void UnlockClingy() => manager.RequestUnlock(clingyId);

    public void RollUpClingy() => manager.RequestRollUp(clingyId);

    public void RollDownClingy() => manager.RequestRollDown(clingyId);
}
