
namespace Clingies.Infrastructure.CustomExceptions;

public class AtLeastOneActiveStyleException(string message) : CustomException(message);