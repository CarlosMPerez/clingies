using System.Data;
using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class ClingyRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IClingyRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();
    public List<Clingy> GetAllActive()
    {
        try
        {
            List<Clingy> clingies = new List<Clingy>();
            var sql = """
                SELECT Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsLocked,
                    PositionX, PositionY, Width, Height
                FROM Clingies
                WHERE IsDeleted = 0
            """;
            var dtos = Conn.Query<ClingyDto>(sql).ToList();
            clingies = dtos.Select(dto => ClingyEntityFactory.FromDto(dto)).ToList();

            return clingies;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllActive");
            throw;
        }
    }
    public Clingy? Get(Guid id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsLocked,
                    PositionX, PositionY, Width, Height
                FROM Clingies
                WHERE Id = @Id
            """;
            var dtos = Conn.Query<ClingyDto>(sql, parms);
            var clingy = dtos.Select(dto => ClingyEntityFactory.FromDto(dto)).FirstOrDefault();

            return clingy;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at Get {0}", id);
            throw;
        }
    }

    public Guid Create(Clingy clingy)
    {
        try
        {
            var sql = """
                INSERT INTO Clingies (Id, Title, Content, CreatedAt, ModifiedAt, 
                    IsDeleted, IsPinned, IsRolled, IsLocked, 
                    PositionX, PositionY, Width, Height)
                VALUES (@Id, @Title, @Content, @CreatedAt, @ModifiedAt, 
                    @IsDeleted, @IsPinned, @IsRolled, @IsLocked, 
                    @PositionX, @PositionY, @Width, @Height)
                """;

            Conn.Execute(sql, clingy);
            return clingy.Id;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating new clingy");
            throw;
        }
    }

    public Guid Update(Clingy clingy)
    {
        try
        {
            var sql = """
                UPDATE Clingies SET 
                    Title = @Title, 
                    Content = @Content, 
                    ModifiedAt = @ModifiedAt, 
                    IsDeleted = @IsDeleted, 
                    IsPinned = @IsPinned,
                    IsRolled = @IsRolled,
                    IsLocked = @IsLocked,
                    PositionX = @PositionX, 
                    PositionY = @PositionY, 
                    Width = @Width, 
                    Height = @Height
                WHERE Id = @Id
                """;
            Conn.Execute(sql, clingy);
            return clingy.Id;
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
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                DELETE FROM Clingies 
                WHERE Id = @Id
                """;

            Conn.Execute(sql, parms);
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
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                UPDATE Clingies SET 
                    IsDeleted = 1 
                WHERE Id = @Id
                """;

            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error soft deleting clingy");
            throw;
        }
    }
}
