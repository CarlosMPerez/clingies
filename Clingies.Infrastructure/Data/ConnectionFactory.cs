using System.Data;
using Microsoft.Data.Sqlite;

namespace Clingies.Infrastructure.Data;

public class ConnectionFactory(string dbPath) : IConnectionFactory
{
    private readonly string connectionString = $"Data Source={dbPath}";

    public IDbConnection CreateConnection()
    {
        Console.WriteLine("connection string: " + connectionString);
        return new SqliteConnection(connectionString);
    }
}
