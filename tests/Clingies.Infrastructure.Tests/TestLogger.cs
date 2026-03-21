using Clingies.Application.Interfaces;

namespace Clingies.Infrastructure.Tests;

internal sealed class TestLogger : IClingiesLogger
{
    public List<string> Warnings { get; } = new();
    public List<string> Infos { get; } = new();
    public List<string> Debugs { get; } = new();
    public List<(Exception Exception, string Message)> Errors { get; } = new();

    public void Debug(string message, params object[] args) =>
        Debugs.Add(string.Format(message, args));

    public void Info(string message, params object[] args) =>
        Infos.Add(string.Format(message, args));

    public void Error(Exception ex, string message, params object[] args) =>
        Errors.Add((ex, string.Format(message, args)));

    public void Warning(string message, params object[] args) =>
        Warnings.Add(args.Length == 0 ? message : string.Format(message, args));
}
