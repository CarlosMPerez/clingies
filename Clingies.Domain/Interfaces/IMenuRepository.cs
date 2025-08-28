using Clingies.Domain.Models;

namespace Clingies.Domain.Interfaces;

public interface IMenuRepository
{
    public List<TrayMenuItem> GetAllParents(string menuType);
    public List<TrayMenuItem> GetChildren(string parentId);
}
