using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IClingyRepository
{
    Task<List<Clingy>> GetAllActiveAsync();
    Task<Clingy> GetAsync(Guid id);
    Task SaveAsync(Clingy clingy);
    Task UpdateAsync(Guid id, Clingy clingy);
    Task SoftDeleteAsync(Guid id);
    Task HardDeleteAsync(Guid id);

}