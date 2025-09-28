using Clingies.Domain.Models;

namespace Clingies.Application.Interfaces;

public interface IClingyRepository
{
    List<ClingyModel> GetAllActive();
    ClingyModel? Get(int id);
    int Create(ClingyModel clingy);
    void Update(ClingyModel clingy);
    void SoftDelete(int id);
    void UnDelete(int id);
    void HardDelete(int id);

}