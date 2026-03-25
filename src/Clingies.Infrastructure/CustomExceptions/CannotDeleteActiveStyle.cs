
namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteActiveStyle(string message) : CustomException(message);