
namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteActiveStyle : CustomException
{
    public CannotDeleteActiveStyle(string message) : base(message) { }
}
