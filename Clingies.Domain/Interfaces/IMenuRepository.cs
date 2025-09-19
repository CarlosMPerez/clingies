using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IMenuRepository
{
    public List<MenuItem> GetAllParents(string menuType);
    public List<MenuItem> GetChildren(string parentId);
}
