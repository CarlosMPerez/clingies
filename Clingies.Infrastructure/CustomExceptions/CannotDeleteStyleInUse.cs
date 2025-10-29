using System;

namespace Clingies.Infrastructure.CustomExceptions;

public class CannotDeleteStyleInUse : CustomException
{
    public CannotDeleteStyleInUse(string message) : base(message) { }
}