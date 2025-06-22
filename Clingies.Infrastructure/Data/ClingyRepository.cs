using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory) : IClingyRepository
{
    public List<Clingy> GetAllActive()
    {
        throw new NotImplementedException();
    }

    public Clingy Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public void HardDelete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Create(Clingy clingy)
    {
        using var conn = connectionFactory.GetConnection();
        var sql = """
            INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted, 
                                PositionX, PositionY, Width, Height)
            VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAt, @IsDeleted, 
                                @PositionX, @PositionY, @Width, @Height)
            """;
        conn.Execute(sql, clingy);
    }

    public void SoftDelete(Guid id)
    {
        throw new NotImplementedException();
    }

    public void Update(Clingy clingy)
    {
        using var conn = connectionFactory.GetConnection();
        var sql = """
            UPDATE Clingies SET 
                Title = @Title, 
                Content = @Content, 
                ModifiedAt = @ModifiedAt, 
                IsDeleted = @IsDeleted, 
                PositionX = @PositionX, 
                PositionY = @PositionY, 
                Width = @Width, 
                Height = @Height
            WHERE Id = @Id
            """;
        conn.Execute(sql, clingy);

    }
}
