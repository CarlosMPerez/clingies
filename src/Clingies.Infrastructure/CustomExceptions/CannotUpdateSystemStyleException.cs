namespace Clingies.Infrastructure.CustomExceptions;

public class CannotUpdateSystemStyleException(string message) : CustomException(message);