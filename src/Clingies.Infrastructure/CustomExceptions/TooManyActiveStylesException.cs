namespace Clingies.Infrastructure.CustomExceptions;

public class TooManyActiveStylesException(string message) : CustomException(message);