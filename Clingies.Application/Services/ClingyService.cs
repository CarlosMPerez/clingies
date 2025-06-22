using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyService(IClingyRepository repo)
{
    public Clingy Create(string title = "", string content = "")
    {
        var clingy = ClingyFactory.CreateNew(title, content);
        repo.Create(clingy);
        return clingy;
    }

    public void Update(Clingy clingy)
    {
        repo.Update(clingy);
    }

}
