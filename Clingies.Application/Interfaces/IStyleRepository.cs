using Clingies.Domain.Models;

namespace Clingies.Application.Interfaces;

public interface IStyleRepository
{
    public List<StyleModel> GetAll();
    public List<StyleModel> GetAllActive();
    public StyleModel? Get(int id);
    public StyleModel? GetDefault();
    public void Create(StyleModel style);
    public void Update(StyleModel style);
    public void Delete(int id);

    public void MarkActive(int id, bool isActive);
    public void MarkDefault(int id, bool isDefault);

    public int GetSystemStyleId();
}
