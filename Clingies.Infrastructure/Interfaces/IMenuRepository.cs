using Clingies.Infrastructure.Models;

namespace Clingies.Infrastructure.Interfaces;

public interface IMenuRepository
{
    public List<MenuItem> GetAllParents(string menuType);
    public List<MenuItem> GetChildren(string parentId);
}
