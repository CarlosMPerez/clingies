namespace Clingies.Infrastructure.CustomExceptions;

public class ReservedStyleNameException(string message) : CustomException(message);