using System.Data;
using Clingies.Domain.Interfaces;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class IconPathRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IIconPathRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();
    public string? GetDarkPath(string id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                SELECT dark_path FROM system_icon_path 
                WHERE id = @id
            """;
            return Conn.ExecuteScalar<string>(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetDarkPath");
            throw;
        }
    }

    public string? GetLightPath(string id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                SELECT light_path FROM system_icon_path 
                WHERE id = @id
            """;
            return Conn.ExecuteScalar<string>(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetLightPath");
            throw;
        }
    }
}
