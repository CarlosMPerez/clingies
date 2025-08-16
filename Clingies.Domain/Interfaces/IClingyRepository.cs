using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IClingyRepository
{
    List<Clingy> GetAllActive();
    Clingy? Get(int id);
    int Create(Clingy clingy);
    int Update(Clingy clingy);
    void SoftDelete(int id);
    void HardDelete(int id);

}