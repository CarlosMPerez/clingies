using System.Data;
using System.Reflection;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Domain.DTOs;
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

                SELECT c.id, c.title, c.type_id, c.created_at, c.is_deleted,
                p.position_x, p.position_y, p.width, p.height, p.is_pinned, p.is_rolled, 
                p.is_locked, p.is_standing, cc.text, cc.png
                FROM clingies AS c
                JOIN clincy_properties as p ON p.id = c.id
                JOIN clingy_content as cc on cc.id = c.id
                WHERE c.is_deleted = 0 AND c.type_id = 1
            """;
            var dtos = Conn.Query<ClingyDto>(sql).ToList();
            clingies = dtos.Select(dto => dto.ToEntity()).ToList();

            return clingies;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at GetAllActive");
            throw;
        }
    }
    public Clingy? Get(int id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT c.id, c.title, c.type_id, c.created_at, c.is_deleted,
                p.position_x, p.position_y, p.width, p.height, p.is_pinned, p.is_rolled, 
                p.is_locked, p.is_standing, cc.text, cc.png
                FROM clingies AS c
                JOIN clincy_properties as p ON p.id = c.id
                JOIN clingy_content as cc on cc.id = c.id
                WHERE c.id = @Id
            """;
            var result = Conn.Query<ClingyDto>(sql, parms);
            var clingy = dtos.Select(dto => dto.ToEntity()).FirstOrDefault();

            return clingy;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at Get {0}", id);
            throw;
        }
    }

    public int Create(Clingy clingy)
    {
        Conn.BeginTransaction();
        try
        {


            var sql = """
                INSERT INTO Clingies (Title, Content, PositionX, PositionY, Width, Height, 
                    IsPinned, IsRolled, IsLocked, IsStanding, IsDeleted, CreatedAt)
                VALUES (@Title, @Content, @PositionX, @PositionY, @Width, @Height, 
                    @IsPinned, @IsRolled, @IsLocked, @IsStanding, @IsDeleted, @CreatedAt);
                SELECT last_insert_rowid();
                """;

            int id = Conn.ExecuteScalar<int>(sql, clingy);
            return id;
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
            var oldVersion = Get(clingy.Id);
            if (!IsEqual<Clingy>(oldVersion!, clingy))
            {
                var sql = """
                UPDATE Clingies SET 
                    Title = @Title, 
                    Content = @Content, 
                    PositionX = @PositionX, 
                    PositionY = @PositionY, 
                    Width = @Width, 
                    Height = @Height, 
                    IsPinned = @IsPinned, 
                    IsRolled = @IsRolled, 
                    IsLocked = @IsLocked, 
                    IsStanding = @IsStanding, 
                    IsDeleted = @IsDeleted, 
                    CreatedAt = @CreatedAt
                WHERE Id = @Id
                """;
                Conn.Execute(sql, clingy);
            }
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

    public void SoftDelete(int id)
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

    private bool IsEqual<T>(T obj1, T obj2)
    {
        if (obj1 is null || obj2 is null) return false;
        if (ReferenceEquals(obj1, obj2)) return true;

        var type = typeof(T);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Optional: skip non-readable or indexer properties
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                continue;

            if (prop.IsDefined(typeof(IgnoreComparisonFieldAttribute), inherit: true))
                continue;

            var val1 = prop.GetValue(obj1);
            var val2 = prop.GetValue(obj2);

            // Special case: DateTime (ignore tick-level differences)
            if (val1 is DateTime dt1 && val2 is DateTime dt2)
            {
                if (dt1.ToUniversalTime().Date != dt2.ToUniversalTime().Date ||
                    dt1.ToUniversalTime().Hour != dt2.ToUniversalTime().Hour ||
                    dt1.ToUniversalTime().Minute != dt2.ToUniversalTime().Minute ||
                    dt1.ToUniversalTime().Second != dt2.ToUniversalTime().Second)
                {
                    return false;
                }
            }
            else if (!Equals(val1, val2))
            {
                return false;
            }
        }

        return true;
    }
}
