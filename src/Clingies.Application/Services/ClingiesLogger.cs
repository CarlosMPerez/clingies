using Clingies.Application.Interfaces;
using Serilog;


namespace Clingies.Application.Services;

public class ClingiesLogger : IClingiesLogger
{
    void IClingiesLogger.Debug(string message, params object[] args) =>
        Log.Logger.Debug(message, args);
    void IClingiesLogger.Error(Exception ex, string message, params object[] args) =>
        Log.Logger.Error(ex, message, args);

    void IClingiesLogger.Info(string message, params object[] args) =>
        Log.Logger.Information(message, args);

    void IClingiesLogger.Warning(string message, params object[] args)
    {
        Log.Logger.Warning(message);
    }
}
