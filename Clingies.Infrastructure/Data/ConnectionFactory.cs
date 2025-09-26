using System.Data;
using Clingies.Application.Interfaces;
using Microsoft.Data.Sqlite;

namespace Clingies.Infrastructure.Data;

public class ConnectionFactory : IConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _sharedConnection;
    private IClingiesLogger _logger;

    public ConnectionFactory(string dbPath, IClingiesLogger logger)
    {
        var csb = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Default
        };
        _connectionString = csb.ToString();
        _logger = logger;
        _logger.Info("Registering connection for db at {0}", dbPath);
    }

    public IDbConnection GetConnection()
    {
        try
        {
            if (_sharedConnection is null)
            {
                _sharedConnection = new SqliteConnection(_connectionString);
                _sharedConnection.Open();
            }

            return _sharedConnection;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error at getting connection");
            throw;
        }
    }

    public void Dispose()
    {
        _logger.Info("Connection disposed");
        _sharedConnection?.Dispose();
    }
}
