using System;

namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteActiveStyle : Exception
{
    public CannotDeleteActiveStyle(string message) : base(message) { }
}
