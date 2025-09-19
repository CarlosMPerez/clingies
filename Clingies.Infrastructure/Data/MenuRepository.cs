using System.Data;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class MenuRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IMenuRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();

    public List<MenuItem> GetAllParents(string menuType)
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
            var items = Conn.Query<MenuItem>(sql, parms).ToList();
            return items;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllParents");
            throw;
        }
    }

    public List<MenuItem> GetChildren(string parentId)
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
            var items = Conn.Query<MenuItem>(sql, parms).ToList();
            return items;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetChildrenByParentId");
            throw;
        }
    }

}
