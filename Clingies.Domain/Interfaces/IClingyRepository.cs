using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IClingyRepository
{
    Task SaveAsync(Clingy clingy);
}
