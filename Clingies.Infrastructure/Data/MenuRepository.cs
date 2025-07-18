using System.Data;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class MenuRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IMenuRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();

    public List<TrayMenuItem> GetAllParents()
    {
        try
        {
            var sql = """
                SELECT * FROM system_tray_menu 
                WHERE parent_id IS NULL
                ORDER BY parent_id, sort_order
            """;
            var items = Conn.Query<TrayMenuItem>(sql).ToList();
            return items;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllParents");
            throw;
        }
    }

    public List<TrayMenuItem> GetChildrenByParentId(string parentId)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@parentId", parentId } };
            var sql = """
                SELECT * FROM system_tray_menu 
                WHERE parent_id = @parentId
                ORDER BY sort_order
            """;
            var items = Conn.Query<TrayMenuItem>(sql, parms).ToList();
            return items;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetChildrenByParentId");
            throw;
        }
    }

}
