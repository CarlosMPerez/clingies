using System.Data;
using Clingies.Domain.Dtos;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Dapper;
using Microsoft.Data.Sqlite;

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
                SELECT id, title, content, position_x, position_y, 
                    width, height, style, is_pinned, is_locked, is_rolled, 
                    is_deleted,  created_at, modified_at
                FROM clingies
                WHERE is_deleted = 0
            """;
            var dtos = Conn.Query<ClingyDto>(sql);
            clingies = dtos.Select(dto => dto.ToEntity()).ToList();

            return clingies;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at ClingyRepository.GetAllActive");
            throw;
        }
    }
    public Clingy? Get(int id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                SELECT id, title, content, position_x, position_y, 
                    width, height, style, is_pinned, is_locked, is_rolled, 
                    is_deleted,  created_at, modified_at
                FROM clingies
                WHERE id = @id
            """;
            var dtos = Conn.Query<ClingyDto>(sql, parms);
            var clingy = dtos.Select(dto => dto.ToEntity()).FirstOrDefault();

            return clingy;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at ClingyRepository.Get {0}", id);
            throw;
        }
    }

    public int Create(Clingy clingy)
    {
        try
        {
            var dto = clingy.ToDto();
            var sql = """
                INSERT INTO clingies (title, content, position_x, position_y, 
                    width, height, style, is_pinned, is_locked, is_rolled, is_deleted,
                    created_at, modified_at)
                VALUES (@title, @content, @position_x, @position_y, 
                    @width, @height, @style, @is_pinned, @is_locked, @is_rolled, @is_deleted,
                    @created_at, @modified_at);
                """;

            Conn.Execute(sql, dto);
            sql = "select last_insert_rowid();";
            return (int)Conn.ExecuteScalar<long>(sql);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating new clingy");
            throw;
        }
    }

    public int Update(Clingy clingy)
    {
        try
        {
            var dto = clingy.ToDto();
            var sql = """
                UPDATE clingies SET 
                    title = @title, 
                    content = @content, 
                    position_x = @position_x, 
                    position_y = @position_y, 
                    width = @width, 
                    height = @height,
                    style = @style,
                    is_pinned = @is_pinned,
                    is_locked = @is_locked,
                    is_rolled = @is_rolled,
                    is_deleted = @is_deleted, 
                    modified_at = @modified_at
                WHERE id = @id
                """;
            Conn.Execute(sql, dto);
            return clingy.Id;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating clingy");
            throw;
        }
    }

    public void HardDelete(int id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                DELETE FROM clingies 
                WHERE id = @id
                """;

            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error hard deleting clingy");
            throw;
        }
    }

    public void SoftDelete(int id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                UPDATE clingies SET 
                    is_deleted = 1 
                WHERE id = @id
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
