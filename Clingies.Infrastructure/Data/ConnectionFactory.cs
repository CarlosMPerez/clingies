using System.Data;
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
        if (_sharedConnection is null)
        {
            _sharedConnection = new SqliteConnection(_connectionString);
            _sharedConnection.Open();
        }

        return _sharedConnection;
    }

    public void Dispose()
    {
        Console.WriteLine("CONNECTION DISPOSED OK");
        _sharedConnection?.Dispose();
    }
}
