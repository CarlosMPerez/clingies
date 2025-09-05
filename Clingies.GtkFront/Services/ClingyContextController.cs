using System;
using System.Linq;
using Clingies.ApplicationLogic.Interfaces;
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

    public void LockClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowAlarmWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowChangeTitleDialog()
    {
        var prevTitle = _manager.ActiveClingies.Where(x => x.Id == _clingyId).First().Title;
        var owner = _manager.ActiveWindows.Where(x => x.ClingyId == _clingyId).First();
        var newTitle = _titleDialog.ShowChangeTitleDialog(owner, prevTitle);
        if (newTitle == null) return;
        // APPLY AND PERSIST
        Console.WriteLine($"NEW TITLE: {newTitle}");
    }

    public void ShowColorWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowPropertiesWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void SleepClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void UnlockClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");
}
