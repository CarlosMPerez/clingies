using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyNoteService(IClingyRepository repo)
{
    public List<Clingy> GetAllActive()
    {
        return repo.GetAllActive();
    }

    public Clingy Get(Guid id)
    {
        return repo.Get(id);
    }

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

    public void SoftDelete(Guid id)
    {
        repo.SoftDelete(id);
    }

    public void HardDelete(Guid id)
    {
        repo.HardDelete(id);
    }

}
