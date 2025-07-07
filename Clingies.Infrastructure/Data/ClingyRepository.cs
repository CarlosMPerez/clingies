using Clingies.Common;
using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IClingyRepository
{
    public List<Clingy> GetAllActive()
    {
        try
        {
            using var conn = connectionFactory.GetConnection();
            List<Clingy> clingies = new List<Clingy>();
            var sql = """
                SELECT Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsStand,
                    PositionX, PositionY, Width, Height
                FROM Clingies
                WHERE IsDeleted = 0
            """;
            var dtos = conn.Query<ClingyDto>(sql).ToList();
            clingies = dtos.Select(dto => ClingyEntityFactory.FromDto(dto)).ToList();

            return clingies;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllActive");
            throw;
        }
    }

    public Clingy Get(Guid id)
    {
        try
        {
            using var conn = connectionFactory.GetConnection();
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsStand,
                    PositionX, PositionY, Width, Height
                FROM Clingies
                WHERE Id = @Id
            """;
            var dtos = conn.Query<ClingyDto>(sql, parms);
            var clingy = dtos.Select(dto => ClingyEntityFactory.FromDto(dto)).First();

            return clingy;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at Get {0}", id);
            throw;
        }
    }

    public void Create(Clingy clingy)
    {
        try
        {
            using var conn = connectionFactory.GetConnection();
            var sql = """
                INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsStand, 
                    PositionX, PositionY, Width, Height)
                VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAt, 
                    @IsDeleted, @IsPinned, @IsRolled, @IsStand, 
                    @PositionX, @PositionY, @Width, @Height)
                """;

            conn.Execute(sql, clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating new clingy");
            throw;
        }
    }

    public void Update(Clingy clingy)
    {
        try
        {
            using var conn = connectionFactory.GetConnection();
            var sql = """
                UPDATE Clingies SET 
                    Title = @Title, 
                    Content = @Content, 
                    ModifiedAt = @ModifiedAt, 
                    IsDeleted = @IsDeleted, 
                    IsPinned = @IsPinned,
                    IsRolled = @IsRolled,
                    IsStand = @IsStand,
                    PositionX = @PositionX, 
                    PositionY = @PositionY, 
                    Width = @Width, 
                    Height = @Height
                WHERE Id = @Id
                """;
            conn.Execute(sql, clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating clingy");
            throw;
        }
    }

    public void HardDelete(Guid id)
    {
        try
        {
            using var conn = connectionFactory.GetConnection();
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                DELETE FROM Clingies 
                WHERE Id = @Id
                """;

            conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error hard deleting clingy");
            throw;
        }
    }

    public void SoftDelete(Guid id)
    {
        try
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
        catch (Exception ex)
        {
            logger.Error(ex, "Error soft deleting clingy");
            throw;
        }
    }
}
