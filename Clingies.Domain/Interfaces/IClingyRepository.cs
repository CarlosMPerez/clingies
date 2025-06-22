using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IClingyRepository
{
    List<Clingy> GetAllActive();
    Clingy Get(Guid id);
    void Create(Clingy clingy);
    void Update(Clingy clingy);
    void SoftDelete(Guid id);
    void HardDelete(Guid id);

}