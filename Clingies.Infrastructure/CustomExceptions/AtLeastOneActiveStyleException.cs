
namespace Clingies.Infrastructure.CustomExceptions;

public class AtLeastOneActiveStyleException : CustomException
{
    public AtLeastOneActiveStyleException(string message) : base(message) { }
}
