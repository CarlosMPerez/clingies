using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IMenuRepository
{
    public List<SystemMenu> GetAllParents(string menuType);
    public List<SystemMenu> GetChildrenByParentId(string parentId);
}
