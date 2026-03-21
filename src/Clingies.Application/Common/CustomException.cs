namespace Clingies.Application.Common;

public class CustomException : Exception
{
    public CustomException(string message) : base(message) { }
}
