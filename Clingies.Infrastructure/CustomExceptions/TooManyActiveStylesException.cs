
namespace Clingies.Infrastructure.CustomExceptions;

public class TooManyActiveStylesException : Exception
{
    public TooManyActiveStylesException(string message) : base(message) { }
}
