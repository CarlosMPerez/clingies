namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteSystemStyleException(string message) : CustomException(message);