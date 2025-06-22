using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyService(IClingyRepository repo)
{
    public async Task<Clingy> CreateAsync(string title = "", string content = "")
    {
        var clingy = ClingyFactory.CreateNew(title, content);
        await repo.SaveAsync(clingy);
        return clingy;
    }

}
