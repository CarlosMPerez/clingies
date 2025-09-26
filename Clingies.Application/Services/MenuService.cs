using Clingies.Application.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class MenuService(IMenuRepository repo, IClingiesLogger logger)
{
    public List<MenuItemModel> GetAllParents(string menuType)
    {
        try
        {
            return repo.GetAllParents(menuType).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error retrieving all Menu Parents for {menuType}");
            throw;
        }
    }

    public List<MenuItemModel> GetChildren(string parentId)
    {
        try
        {
            return repo.GetChildren(parentId).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error retrieving all Menu Children for {parentId}");
            throw;
        }
    }
}
