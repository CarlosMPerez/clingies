using System.Data;
using Clingies.Application.Interfaces;
using Clingies.Domain.Models;
using Clingies.Infrastructure.Entities;
using Clingies.Infrastructure.Mapper;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class MenuRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IMenuRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();

    public List<MenuItemModel> GetAllParents(string menuType)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@menuType", menuType } };
            var sql = """
                SELECT * FROM system_menu 
                WHERE menu_type = @menuType
                AND parent_id IS NULL
                ORDER BY sort_order
            """;
            var items = Conn.Query<MenuItemEntity>(sql, parms).ToList();
            return items.Select(entity => entity.ToModel()).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllParents");
            throw;
        }
    }

    public List<MenuItemModel> GetChildren(string parentId)
    {
        try
        {
            // menu_type is not needed because it's determined by the parent menu
            var parms = new Dictionary<string, object> { { "@parentId", parentId } };
            var sql = """
                SELECT * FROM system_menu 
                WHERE parent_id = @parentId
                ORDER BY sort_order
            """;
            var items = Conn.Query<MenuItemEntity>(sql, parms).ToList();
            return items.Select(entity => entity.ToModel()).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetChildrenByParentId");
            throw;
        }
    }

}
