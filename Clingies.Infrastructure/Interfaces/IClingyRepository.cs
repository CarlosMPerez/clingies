using Clingies.Infrastructure.Models;

namespace Clingies.Infrastructure.Interfaces;

public interface IClingyRepository
{
    List<Clingy> GetAllActive();
    Clingy? Get(int id);
    int Create(Clingy clingy);
    void Update(Clingy clingy);
    void SoftDelete(int id);
    void HardDelete(int id);

}