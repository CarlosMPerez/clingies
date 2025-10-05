
namespace Clingies.Infrastructure.CustomExceptions;

public class CannotUpdateSystemStyleException: CustomException
{
    public CannotUpdateSystemStyleException(string message) : base(message) {}
}
