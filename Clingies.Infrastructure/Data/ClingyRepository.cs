using System.Data;
using System.Reflection;
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
            var sql =
                """
                    SELECT c.id, c.title, c.type_id AS Type, c.created_at, c.is_deleted,
                    p.Id as PropsId, p.position_x, p.position_y, 
                    p.width, p.height, p.is_pinned, p.is_rolled, 
                    p.is_locked, p.is_standing, 
                    cc.Id as ContentId, cc.text, cc.png
                    FROM clingies AS c
                    JOIN clingy_properties as p ON p.id = c.id
                    JOIN clingy_content as cc on cc.id = c.id
                    WHERE c.is_deleted = 0 AND c.type_id = @TypeId;
                """;
            var data = Conn.Query<Clingy, ClingyProperties, ClingyContent, Clingy>(
                sql,
                (c, p, ct) =>
                {
                    p.Id = c.Id;
                    c.Id = p.Id;
                    c.Properties = p;
                    c.Content = ct;
                    return c;
                },
                new { TypeId = (int)Enums.ClingyType.Desktop },
                splitOn: "PropsId, ContentId"
            ).ToList();

            return data;
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
            var sql =
                """
                    SELECT c.id, c.type_id AS Type, c.title, c.created_at, c.is_deleted,
                    p.Id as Id, p.position_x, p.position_y, p.width, 
                    p.height, p.is_pinned, p.is_rolled, 
                    p.is_locked, p.is_standing, 
                    c.Id as Id, cc.text, cc.png
                    FROM clingies AS c
                    JOIN clingy_properties AS p ON p.id = c.id
                    JOIN clingy_content AS cc ON cc.id = c.id
                    WHERE c.id = @Id;
                """;
            return Conn.Query<Clingy, ClingyProperties, ClingyContent, Clingy>(
                sql,
                (c, p, ct) =>
                {
                    c.Properties = p;
                    c.Content = ct;
                    return c;
                },
                new { Id = id },
                splitOn: "Id, Id"
            ).SingleOrDefault();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at Get {0}", id);
            throw;
        }
    }

    public int Create(Clingy clingy)
    {
        using var tx = Conn.BeginTransaction();
        try
        {
            var newId = Conn.ExecuteScalar<long>(
                @"INSERT INTO clingies (type_id, title, created_at, is_deleted)
                  VALUES (@TypeId, @Title, @CreatedAt, 0);
                  SELECT last_insert_rowid();",
                new
                {
                    TypeId = clingy.Type,
                    Title = clingy.Title,
                    CreatedAt = clingy.CreatedAt == default ? DateTime.UtcNow : clingy.CreatedAt
                },
                tx
            );
            clingy.Id = (int)newId;

            clingy.Properties.Id = clingy.Id;
            Conn.Execute(
                """
                    INSERT INTO clingy_properties
                    (id, position_x, position_y, width, height, is_pinned, is_rolled, is_locked, is_standing)
                    VALUES
                    (@Id, @PositionX, @PositionY, @Width, @Height, @IsPinned, @IsRolled, @IsLocked, @IsStanding);
                """,
                clingy.Properties, tx
            );

            // NEW clingies are ALWAYS created with content as null, either TEXT or PNG
            clingy.Content.Id = clingy.Id;
            Conn.Execute(
                """
                    INSERT INTO clingy_content (id, text, png)
                    VALUES (@Id, null, null);
                """,
                clingy.Content, tx
            );

            tx.Commit();
            return clingy.Id;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.Error(ex, "Error creating new clingy");
            throw;
        }
    }

    public void Update(Clingy incoming)
    {
        if (incoming is null) throw new ArgumentNullException(nameof(incoming));
        if (incoming.Id <= 0) throw new ArgumentOutOfRangeException(nameof(incoming.Id));

        using var tx = Conn.BeginTransaction();
        try
        {
            var current = LoadAggregateForUpdate(incoming.Id, tx)
                          ?? throw new KeyNotFoundException($"Clingy {incoming.Id} not found.");

            // Compute dirtiness
            var mainDirty = !IsEqual(current, incoming);
            var propsDirty = !IsEqual(current.Properties!, incoming.Properties!);
            var contDirty = !IsEqual(current.Content!, incoming.Content!);

            if (mainDirty || propsDirty || contDirty)
            {
                // Normalize “empties”
                NormalizeContent(incoming.Content);
                NormalizeContent(current.Content);

                // Enforce XOR ONLY on update
                var hasText = incoming.Content?.Text is { Length: > 0 };
                var hasImg = incoming.Content?.Png is { Length: > 0 };
                if (hasText && hasImg) // if both are true, error
                    throw new InvalidOperationException("On update, content must be either Text or Png (exclusively).");

                if (mainDirty) UpdateMain(incoming, tx);
                if (propsDirty) UpdateProps(incoming, tx);
                if (contDirty) UpdateContent(incoming, tx);
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            logger.Error(ex, "Error updating clingy");
            throw;
        }
    }

    public void HardDelete(int id)
    {
        try
        {
            // children cascade via FK
            var rows = Conn.Execute(
                """
                    DELETE FROM clingies WHERE id = @Id;
                """,
                new { Id = id });
            EnsureFound(rows, id, "clingies");
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
            var rows = Conn.Execute(
                """
                    UPDATE clingies SET is_deleted = 1 WHERE id = @Id;
                """,
                new { Id = id });
            EnsureFound(rows, id, "clingies");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error soft deleting clingy");
            throw;
        }
    }

    private void NormalizeContent(ClingyContent? content)
    {
        if (content is null) return;
        if (string.IsNullOrWhiteSpace(content.Text)) content.Text = null;
        if (content.Png is { Length: 0 }) content.Png = null;
    }

    private void UpdateMain(Clingy main, IDbTransaction tx)
    {
        var rows = Conn.Execute(
            """
                UPDATE clingies
                SET type_id = @TypeId,
                    title = @Title
                WHERE id = @Id;
            """,
            new { Id = main.Id, TypeId = (int)main.Type, Title = main.Title }, tx);
        EnsureFound(rows, main.Id, "clingies");
    }

    private void UpdateProps(Clingy main, IDbTransaction tx)
    {
        var props = main.Properties!;
        var rows = Conn.Execute(
            """
                UPDATE clingy_properties
                SET position_x=@PositionX, position_y=@PositionY,
                    width=@Width, height=@Height,
                    is_pinned=@IsPinned, is_rolled=@IsRolled,
                    is_locked=@IsLocked, is_standing=@IsStanding
                WHERE id=@Id;
            """,
            new
            {
                Id = main.Id,
                props.PositionX,
                props.PositionY,
                props.Width,
                props.Height,
                props.IsPinned,
                props.IsRolled,
                props.IsLocked,
                props.IsStanding
            }, tx);
        EnsureFound(rows, main.Id, "clingy_properties");
    }

    private void UpdateContent(Clingy main, IDbTransaction tx)
    {
        var hasText = !string.IsNullOrEmpty(main.Content!.Text);
        int rows;
        if (hasText)
        {
            rows = Conn.Execute(
                """
                    UPDATE clingy_content
                    SET text=@Text, png=NULL
                    WHERE id=@Id;
                """,
                new { Id = main.Id, Text = main.Content!.Text }, tx);
        }
        else
        {
            rows = Conn.Execute(
                """
                    UPDATE clingy_content
                    SET text=NULL, png=@Png
                    WHERE id=@Id;
                """,
                new { Id = main.Id, Png = main.Content!.Png }, tx);
        }
        EnsureFound(rows, main.Id, "clingy_content");
    }

    private static void EnsureFound(int affected, int id, string table)
    {
        if (affected == 0)
            throw new KeyNotFoundException($"No row in '{table}' with id={id}.");
    }

    private Clingy? LoadAggregateForUpdate(int id, IDbTransaction tx)
    {
        const string sql =
            """
                SELECT c.id, c.type_id AS Type, c.title, c.created_at, c.is_deleted,
                p.Id as Id, p.position_x, p.position_y, p.width, 
                p.height, p.is_pinned, p.is_rolled, 
                p.is_locked, p.is_standing, 
                cc.Id as Id, cc.text, cc.png
                FROM clingies AS c
                JOIN clingy_properties AS p ON p.id = c.id
                JOIN clingy_content AS cc ON cc.id = c.id
                WHERE c.id = @Id;
            """;
        var x = Conn.Query<Clingy, ClingyProperties, ClingyContent, Clingy>(
            sql,
            (c, p, ct) =>
            {
                c.Properties = p;
                c.Content = ct;
                return c;
            },
            new { Id = id }, tx,
            splitOn: "Id, Id"
        ).SingleOrDefault();
        return x;
    }

    private bool IsEqual<T>(T obj1, T obj2)
    {
        if (obj1 is null || obj2 is null) return false;
        if (ReferenceEquals(obj1, obj2)) return true;

        var t1 = obj1.GetType();
        var t2 = obj2.GetType();
        if (t1 != t2) return false; // different runtime types

        foreach (var prop in t1.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Optional: skip non-readable or indexer properties
            if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                continue;

            if (prop.IsDefined(typeof(IgnoreComparisonFieldAttribute), inherit: true))
                continue;

            var val1 = prop.GetValue(obj1);
            var val2 = prop.GetValue(obj2);

            // normalize       
            if (val1 is byte[] ba && ba.Length == 0) val1 = null;
            if (val2 is byte[] bb && bb.Length == 0) val2 = null;

            // both null => equal
            if (val1 is null && val2 is null) continue;
            // one null => different
            if (val1 is null || val2 is null) return false;

            // BLOBs: byte[] content compare (not reference)
            if (val1 is byte[] b1 && val2 is byte[] b2)
            {
                if (!b1.AsSpan().SequenceEqual(b2)) return false;
                continue;
            }

            // DateTime: compare up to whole seconds in UTC
            if (val1 is DateTime dt1 && val2 is DateTime dt2)
            {
                dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Utc).ToUniversalTime();
                dt2 = DateTime.SpecifyKind(dt2, DateTimeKind.Utc).ToUniversalTime();
                if (dt1.Year != dt2.Year || dt1.Month != dt2.Month || dt1.Day != dt2.Day ||
                    dt1.Hour != dt2.Hour || dt1.Minute != dt2.Minute || dt1.Second != dt2.Second)
                    return false;
                continue;
            }

            // Strings: optional normalization (treat null/empty/whitespace the same)
            if (val1 is string s1 && val2 is string s2)
            {
                var n1 = string.IsNullOrWhiteSpace(s1) ? string.Empty : s1;
                var n2 = string.IsNullOrWhiteSpace(s2) ? string.Empty : s2;
                if (!string.Equals(n1, n2, StringComparison.Ordinal)) return false;
                continue;
            }

            // Fallback to Equals (covers primitives, enums, bools, etc.)
            if (!Equals(val1, val2)) return false;
        }

        return true;
    }
}
