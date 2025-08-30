using System;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.GtkFront.Factories;

namespace Clingies.GtkFront.Windows.Parts;

public class ClingyContextController : IContextCommandController
{
    private readonly ClingyWindowManager _manager;
    private readonly int _clingyId;

    public ClingyContextController(ClingyWindowManager manager, int clingyId)
    {
        _manager = manager;
        _clingyId = clingyId;
    }

    public void LockClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowAlarmWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowChangeTitleDialog() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowColorWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void ShowPropertiesWindow() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void SleepClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");

    public void UnlockClingy() => Console.WriteLine("TO BE IMPLEMENTED BY WINDOW CONTEXT MENU");
}
