
namespace Clingies.Infrastructure.CustomExceptions;

public class TooManyActiveStylesException : CustomException
{
    public TooManyActiveStylesException(string message) : base(message) { }
}
