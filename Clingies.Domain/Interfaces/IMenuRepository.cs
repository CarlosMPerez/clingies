using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IMenuRepository
{
    public List<TrayMenuItem> GetAllParents();
    public List<TrayMenuItem> GetChildrenByParentId(string parentId);
}
