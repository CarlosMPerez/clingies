using System.Collections.Generic;
using Clingies.Application.Factories;
using Clingies.Application.Services;
using Clingies.Domain.Models;
using Clingies.App.Windows;

namespace Clingies.App.Factories;

public class ClingyWindowFactory(ClingyNoteService noteService) : IClingyWindowFactory
{
    private List<ClingyWindow> activeWindows = new List<ClingyWindow>();
    public void CreateNewWindow(Clingy clingy)
    {
        var window = new ClingyWindow(noteService, clingy);
        activeWindows.Add(window);
        window.Show();
    }

    public void LoadAllActiveWindows(List<Clingy> clingies)
    {
        foreach (var clingy in clingies)
        {
            CreateNewWindow(clingy);            
        }
    }

    public void RenderAllWindows()
    {
        foreach (var window in activeWindows)
        {
            window.Topmost = true;
            window.Activate();
            window.Topmost = false;
        }
    }

}
