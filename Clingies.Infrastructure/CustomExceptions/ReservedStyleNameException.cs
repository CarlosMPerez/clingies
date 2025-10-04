using System;

namespace Clingies.Infrastructure.CustomExceptions;

public class ReservedStyleNameException : Exception
{
    public ReservedStyleNameException(string message) : base(message) {}
}
