using System;

namespace Clingies.Infrastructure.CustomExceptions;

public class CannotUpdateSystemStyleException: Exception
{
    public CannotUpdateSystemStyleException(string message) : base(message) {}
}
