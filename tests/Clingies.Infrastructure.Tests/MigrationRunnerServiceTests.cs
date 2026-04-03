using Dapper;

namespace Clingies.Infrastructure.Tests;

public class MigrationRunnerServiceTests
{
    [Fact]
    public void MigrateUp_CreatesSchemaAndSeedsBaseData()
    {
        using var db = new TestDatabase();

        Assert.Equal(1L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'styles'"));
        Assert.Equal(1L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'clingies'"));
        Assert.Equal(1L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'clingy_properties'"));
        Assert.Equal(1L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'clingy_content'"));
        Assert.Equal(1L, db.Scalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'clingy_types'"));

        Assert.Equal(5, db.Scalar<int>("SELECT COUNT(*) FROM clingy_types"));

        var systemStyle = db.QuerySingle<SystemStyleRow>(
            """
            SELECT id AS Id, style_name AS StyleName, is_system AS IsSystem, is_default AS IsDefault, is_active AS IsActive
            FROM styles
            WHERE is_system = 1
            """);

        Assert.Equal("System", systemStyle.StyleName);
        Assert.True(systemStyle.IsSystem);
        Assert.True(systemStyle.IsDefault);
        Assert.True(systemStyle.IsActive);
    }

    [Fact]
    public void MigrateUp_IsIdempotentForSeedData()
    {
        using var db = new TestDatabase();

        new MigrationRunnerService(db.DbPath).MigrateUp();

        Assert.Equal(5, db.Scalar<int>("SELECT COUNT(*) FROM clingy_types"));
        Assert.Equal(1, db.Scalar<int>("SELECT COUNT(*) FROM styles WHERE is_system = 1"));
        Assert.Equal(1, db.Scalar<int>("SELECT COUNT(*) FROM styles WHERE style_name = 'System'"));
    }

    [Fact]
    public void MigrateUp_CreatesStyleProtectionTriggers()
    {
        using var db = new TestDatabase();

        var triggerNames = db.Query<string>(
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'trigger'
            ORDER BY name
            """).ToList();

        Assert.Contains("trg_styles_prevent_delete_system", triggerNames);
        Assert.Contains("trg_styles_prevent_change_system", triggerNames);
        Assert.Contains("trg_styles_prevent_more_than_one_system", triggerNames);
        Assert.Contains("trg_styles_prevent_id_change_system", triggerNames);
        Assert.Contains("trg_clingy_content_xor_insert", triggerNames);
        Assert.Contains("trg_clingy_content_xor_update", triggerNames);
    }

    [Fact]
    public void SystemStyleTriggers_BlockDeleteAndProtectedUpdates()
    {
        using var db = new TestDatabase();

        var deleteEx = Assert.ThrowsAny<Exception>(() =>
            db.Connection.Execute("DELETE FROM styles WHERE is_system = 1"));
        Assert.Contains("Cannot delete system style", deleteEx.Message);

        var updateEx = Assert.ThrowsAny<Exception>(() =>
            db.Connection.Execute("UPDATE styles SET style_name = 'Changed' WHERE is_system = 1"));
        Assert.Contains("read-only columns", updateEx.Message);
    }

    [Fact]
    public void ContentXorTriggers_BlockInsertWhenTextAndPngAreBothPresent()
    {
        using var db = new TestDatabase();

        var ex = Assert.ThrowsAny<Exception>(() =>
            db.Connection.Execute(
                """
                INSERT INTO clingy_content (id, text, png)
                VALUES (@Id, @Text, @Png)
                """,
                new
                {
                    Id = 999_001,
                    Text = "hello",
                    Png = new byte[] { 1, 2, 3 }
                }));

        Assert.Contains("mutually exclusive", ex.Message);
    }

    [Fact]
    public void ContentXorTriggers_BlockUpdateWhenTextAndPngAreBothPresent()
    {
        using var db = new TestDatabase();
        var clingyId = db.CreateClingy();

        var ex = Assert.ThrowsAny<Exception>(() =>
            db.Connection.Execute(
                """
                UPDATE clingy_content
                SET text = @Text,
                    png = @Png
                WHERE id = @Id
                """,
                new
                {
                    Id = clingyId,
                    Text = "hello",
                    Png = new byte[] { 4, 5, 6 }
                }));

        Assert.Contains("mutually exclusive", ex.Message);
    }

    private sealed class SystemStyleRow
    {
        public int Id { get; init; }
        public string StyleName { get; init; } = string.Empty;
        public bool IsSystem { get; init; }
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; }
    }
}
