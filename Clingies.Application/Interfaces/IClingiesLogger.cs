
namespace Clingies.Application.Interfaces;

public interface IClingiesLogger
{
    void Debug(string message, params object[] args);
    void Info(string message, params object[] args);
    void Error(Exception ex, string message, params object[] args);
    void Warning(string message, params object[] args);
}
