
namespace Clingies.Infrastructure.CustomExceptions;

public class ReservedStyleNameException : CustomException
{
    public ReservedStyleNameException(string message) : base(message) {}
}
