using Clingies.Application.Factories;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyWindowService(ClingyNoteService noteService,
                            IClingyWindowFactory windowFactory)
{
    public void CreateNewWindow(Clingy clingy)
    {
        windowFactory.CreateNewWindow(clingy);
    }

    public void ShowAllActiveClingiesOnTop()
    {
        windowFactory.RenderAllWindows();
    }

    public void LoadAllActiveClingies()
    {
        var clingies = noteService.GetAllActive();
        windowFactory.LoadAllActiveWindows(clingies);
    }
}
