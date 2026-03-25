using System.Data;
using Clingies.Domain.Models;
using Clingies.Infrastructure.Data;
using Dapper;

namespace Clingies.Infrastructure.Tests;

internal sealed class TestDatabase : IDisposable
{
    private readonly string _tempDirectory;
    private readonly TestLogger _logger = new();

    public TestDatabase()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        _tempDirectory = Path.Combine(Path.GetTempPath(), "clingies-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);

        DbPath = Path.Combine(_tempDirectory, "clingies.db");
        new MigrationRunnerService(DbPath).MigrateUp();

        ConnectionFactory = new ConnectionFactory(DbPath, _logger);
        Clingies = new ClingyRepository(ConnectionFactory, _logger);
        Styles = new StyleRepository(ConnectionFactory, _logger);
    }

    public string DbPath { get; }
    public ConnectionFactory ConnectionFactory { get; }
    public ClingyRepository Clingies { get; }
    public StyleRepository Styles { get; }
    public IDbConnection Connection => ConnectionFactory.GetConnection();
    public int SystemStyleId => Styles.GetSystemStyleId();

    public int CreateStyle(string? name = null, bool isActive = true, bool isDefault = false)
    {
        var style = new StyleModel
        {
            StyleName = name ?? $"Style-{Guid.NewGuid():N}",
            BodyColor = "#ABCDEF",
            BodyFontName = "monospace",
            BodyFontColor = "#111111",
            BodyFontSize = 16,
            BodyFontDecorations = Enums.FontDecorations.Bold,
            IsActive = isActive,
            IsDefault = isDefault
        };

        Styles.Create(style);
        return Scalar<int>(
            """
            SELECT id
            FROM styles
            WHERE style_name = @StyleName
            ORDER BY id DESC
            LIMIT 1
            """,
            new { style.StyleName });
    }

    public int CreateClingy(int? styleId = null, string? title = null)
    {
        var clingy = new ClingyModel
        {
            Title = title ?? $"Clingy-{Guid.NewGuid():N}",
            Type = Enums.ClingyType.Desktop,
            StyleId = styleId ?? SystemStyleId
        };

        return Clingies.Create(clingy);
    }

    public T Scalar<T>(string sql, object? param = null) =>
        Connection.ExecuteScalar<T>(sql, param)!;

    public T QuerySingle<T>(string sql, object? param = null) =>
        Connection.QuerySingle<T>(sql, param);

    public IEnumerable<T> Query<T>(string sql, object? param = null) =>
        Connection.Query<T>(sql, param);

    public void Dispose()
    {
        ConnectionFactory.Dispose();

        try
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        catch
        {
            // Best effort cleanup for temp database files.
        }
    }
}
