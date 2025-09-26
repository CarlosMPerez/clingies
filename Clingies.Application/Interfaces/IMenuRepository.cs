using Clingies.Domain.Models;

namespace Clingies.Application.Interfaces;

public interface IMenuRepository
{
    public List<MenuItemModel> GetAllParents(string menuType);
    public List<MenuItemModel> GetChildren(string parentId);
}
