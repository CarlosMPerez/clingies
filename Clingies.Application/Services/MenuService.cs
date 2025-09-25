using Clingies.Domain.Interfaces;
using Clingies.Infrastructure.Interfaces;
using Clingies.Domain.DTOs;

namespace Clingies.Application.Services;

public class MenuService(IMenuRepository repo, IClingiesLogger logger)
{
    public List<MenuItemDto> GetAllParents(string menuType)
    {
        try
        {
            return repo.GetAllParents(menuType).Select(dto => dto.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error retrieving all Menu Parents for {menuType}");
            throw;
        }
    }

    public List<MenuItemDto> GetChildren(string parentId)
    {
        try
        {
            return repo.GetChildren(parentId).Select(dto => dto.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error retrieving all Menu Children for {parentId}");
            throw;
        }
    }
}
