using System.Data;
using Clingies.Common;
using Microsoft.Data.Sqlite;

namespace Clingies.Infrastructure.Data;

public class ConnectionFactory : IConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _sharedConnection;

    public ConnectionFactory(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
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

            //_logger.Info("Connection open");
            return _sharedConnection;
        }
        catch (Exception ex)
        {
            //_logger.Error(ex, "Error opening connection");
            throw;
        }
    }

    public void Dispose()
    {
        //_logger.Info("Connection disposed");
        _sharedConnection?.Dispose();
    }
}
