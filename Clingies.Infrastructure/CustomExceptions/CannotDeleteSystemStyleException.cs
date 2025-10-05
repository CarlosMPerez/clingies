
namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteSystemStyleException : CustomException
{
    public CannotDeleteSystemStyleException(string message) : base(message) { }
}
