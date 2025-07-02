using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory) : IClingyRepository
{
    public List<Clingy> GetAllActive()
    {
        using var conn = connectionFactory.GetConnection();
        List<Clingy> clingies = new List<Clingy>();
        var sql = """
            SELECT Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted, IsPinned, 
                                PositionX, PositionY, Width, Height
            FROM Clingies
            WHERE IsDeleted = 0
        """;
        var dtos = conn.Query<ClingyDto>(sql).ToList();
        clingies = dtos.Select(dto => ClingyFactory.FromDto(dto)).ToList();

        return clingies;
    }

    public Clingy Get(Guid id)
    {
        using var conn = connectionFactory.GetConnection();
        var parms = new Dictionary<string, object> { { "@Id", id } };        
        var sql = """
            SELECT Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted, IsPinned, 
                                PositionX, PositionY, Width, Height
            FROM Clingies
            WHERE Id = @Id
        """;
        var dtos = conn.Query<ClingyDto>(sql, parms);
        var clingy = dtos.Select(dto => ClingyFactory.FromDto(dto)).First();

        return clingy;
    }

    public void Create(Clingy clingy)
    {
        using var conn = connectionFactory.GetConnection();
        var sql = """
            INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, IsDeleted, IsPinned,
                                PositionX, PositionY, Width, Height)
            VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAt, @IsDeleted, @IsPinned,
                                @PositionX, @PositionY, @Width, @Height)
            """;

        conn.Execute(sql, clingy);
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
                IsPinned = @IsPinned,
                PositionX = @PositionX, 
                PositionY = @PositionY, 
                Width = @Width, 
                Height = @Height
            WHERE Id = @Id
            """;
        conn.Execute(sql, clingy);
    }

    public void HardDelete(Guid id)
    {
        using var conn = connectionFactory.GetConnection();
        var parms = new Dictionary<string, object> { { "@Id", id } };        
        var sql = """
            DELETE FROM Clingies 
            WHERE Id = @Id
            """;

        conn.Execute(sql, parms);
    }

    public void SoftDelete(Guid id)
    {
        using var conn = connectionFactory.GetConnection();
        var parms = new Dictionary<string, object> { { "@Id", id } };        
        var sql = """
            UPDATE Clingies SET 
                IsDeleted = 1 
            WHERE Id = @Id
            """;

        conn.Execute(sql, parms);
    }
}
