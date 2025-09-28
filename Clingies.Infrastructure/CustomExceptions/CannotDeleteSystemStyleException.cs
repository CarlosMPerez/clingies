
namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteSystemStyleException : Exception
{
    public CannotDeleteSystemStyleException(string message) : base(message) { }
}
