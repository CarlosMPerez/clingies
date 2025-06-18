using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.CreateClingy;

public class CreateClingyHandler(IClingyRepository repository)
{
    public async Task<Guid> Handle(CreateClingyCommand command)
    {
        var clingy = Clingy.Create(command.Content, command.Title);
        await repository.SaveAsync(clingy);
        return clingy.Id;
    }
}
