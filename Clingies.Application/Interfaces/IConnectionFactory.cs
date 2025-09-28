using System.Data;

namespace Clingies.Application.Interfaces;

public interface IConnectionFactory
{
    IDbConnection GetConnection();
}
