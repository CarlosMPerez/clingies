using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IClingyRepository
{
    List<Clingy> GetAllActive();
    Clingy? Get(Guid id);
    Guid Create(Clingy clingy);
    Guid Update(Clingy clingy);
    void SoftDelete(Guid id);
    void HardDelete(Guid id);

}