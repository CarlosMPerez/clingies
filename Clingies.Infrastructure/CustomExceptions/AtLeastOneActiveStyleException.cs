
namespace Clingies.Infrastructure.CustomExceptions;

public class AtLeastOneActiveStyleException : Exception
{
    public AtLeastOneActiveStyleException(string message) : base(message) { }
}
