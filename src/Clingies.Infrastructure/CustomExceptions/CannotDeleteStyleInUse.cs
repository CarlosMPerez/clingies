namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteStyleInUse(string message) : CustomException(message);