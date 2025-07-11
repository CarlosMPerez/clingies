using System;
using Serilog;
using Clingies.Common;

namespace Clingies.App.Common;

public class ClingiesLogger : IClingiesLogger
{
    void IClingiesLogger.Debug(string message, params object[] args) =>
        Log.Logger.Debug(message, args);
    void IClingiesLogger.Error(Exception ex, string message, params object[] args) =>
        Log.Logger.Error(ex, message, args);

    void IClingiesLogger.Info(string message, params object[] args) =>
        Log.Logger.Information(message, args);
}
