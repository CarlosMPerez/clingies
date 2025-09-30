using System;
using Clingies.Application.Interfaces;
using Clingies.GtkFront.Windows;

namespace Clingies.GtkFront.Services;

public class ClingyContextController : IContextCommandController
{
    private readonly ClingyWindowManager _manager;
    private readonly ITitleDialogService _titleDialog;
    private readonly int _clingyId;

    public ClingyContextController(ClingyWindowManager manager, ITitleDialogService titleDialog, int clingyId)
    {
        _manager = manager;
        _titleDialog = titleDialog;
        _clingyId = clingyId;
    }

    public void ShowAlarmWindow() => Console.WriteLine("ALARM TO BE IMPLEMENTED");

    public void ShowChangeTitleDialog()
    {
        var prevTitle = _manager.GetClingyModelById(_clingyId)?.Title;
        var owner = _manager.GetClingyWindowById(_clingyId);
        var newTitle = _titleDialog.ShowChangeTitleDialog(owner, prevTitle);
        if (newTitle == null) return;
        _manager.RequestTitleChange(_clingyId, newTitle);
    }

    public void ApplyStyle(int styleId) => Console.WriteLine($"APPLY STYLE {styleId}");

    public void ShowColorWindow() => Console.WriteLine("COLOR TO BE IMPLEMENTED");

    public void ShowPropertiesWindow() => Console.WriteLine("PROPERTIES TO BE IMPLEMENTED");

    public void SleepClingy() => Console.WriteLine("SLEEP TO BE IMPLEMENTED");

    public void LockClingy() => _manager.RequestLock(_clingyId);

    public void UnlockClingy() => _manager.RequestUnlock(_clingyId);

    public void RollUpClingy() => _manager.RequestRollUp(_clingyId);

    public void RollDownClingy() => _manager.RequestRollDown(_clingyId);
}
