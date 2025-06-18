using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory) : IClingyRepository
{
    public async Task SaveAsync(Clingy clingy)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted)
            VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAtm @IsDeleted)
            """;
        await conn.ExecuteAsync(sql, clingy);
    }
}
