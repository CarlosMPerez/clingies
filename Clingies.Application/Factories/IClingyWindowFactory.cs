using Clingies.Domain.Models;

namespace Clingies.Application.Factories;

public interface IClingyWindowFactory
{
    void CreateNewWindow(Clingy clingy);

    void RenderAllWindows();

    void LoadAllActiveWindows(List<Clingy> clingies);
}
