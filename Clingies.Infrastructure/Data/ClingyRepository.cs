using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory) : IClingyRepository
{
    public Task<List<Clingy>> GetAllActiveAsync()
    {
        throw new NotImplementedException();
    }

    public Task GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task HardDeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(Clingy clingy)
    {
        using var conn = connectionFactory.CreateConnection();
        var sql = """
            INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted, 
                                PositionX, PositionY, Width, Height)
            VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAt, @IsDeleted, 
                                @PositionX, @PositionY, @Width, @Height)
            """;
        await conn.ExecuteAsync(sql, clingy);
    }

    public Task SoftDeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid id, Clingy clingy)
    {
        throw new NotImplementedException();
    }

    Task<Clingy> IClingyRepository.GetAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
