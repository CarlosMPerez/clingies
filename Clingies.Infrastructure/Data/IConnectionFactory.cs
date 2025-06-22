using System.Data;

namespace Clingies.Infrastructure.Data;

public interface IConnectionFactory
{
    IDbConnection GetConnection();
}
