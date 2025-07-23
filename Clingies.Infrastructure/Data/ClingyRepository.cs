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
                SELECT id, title, content, created_at, modified_at, 
                    is_deleted, is_pinned, is_rolled, is_locked,
                    position_x, position_y, width, height
                FROM clingies
                WHERE is_deleted = 0
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
                SELECT id, title, content, created_at, modified_at, 
                    is_deleted, is_pinned, is_rolled, is_locked,
                    position_x, position_y, width, height
                FROM clingies
                WHERE id = @Id
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
                INSERT INTO Clingies (id, title, content, created_at, modified_at, 
                    is_deleted, is_pinned, is_rolled, is_locked,
                    position_x, position_y, width, height)
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
                    title = @Title, 
                    content = @Content, 
                    modified_at = @ModifiedAt, 
                    is_deleted = @IsDeleted, 
                    is_pinned = @IsPinned,
                    is_rolled = @IsRolled,
                    is_locked = @IsLocked,
                    position_x = @PositionX, 
                    position_y = @PositionY, 
                    width = @Width, 
                    height = @Height
                WHERE id = @Id
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
                DELETE FROM clingies 
                WHERE id = @Id
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
                UPDATE clingies SET 
                    is_deleted = 1 
                WHERE id = @Id
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
