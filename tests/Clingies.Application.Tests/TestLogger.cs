namespace Clingies.Application.Tests;

internal sealed class TestLogger : IClingiesLogger
{
    public List<string> DebugMessages { get; } = new();
    public List<string> InfoMessages { get; } = new();
    public List<string> WarningMessages { get; } = new();
    public List<(Exception Exception, string Message)> ErrorEntries { get; } = new();

    public void Debug(string message, params object[] args) =>
        DebugMessages.Add(args.Length == 0 ? message : string.Format(message, args));

    public void Info(string message, params object[] args) =>
        InfoMessages.Add(args.Length == 0 ? message : string.Format(message, args));

    public void Error(Exception ex, string message, params object[] args) =>
        ErrorEntries.Add((ex, args.Length == 0 ? message : string.Format(message, args)));

    public void Warning(string message, params object[] args) =>
        WarningMessages.Add(args.Length == 0 ? message : string.Format(message, args));
}
