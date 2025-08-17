using System;

namespace Clingies.ApplicationLogic.Interfaces;

public interface IFrontendHost
{
    /// <summary>Run the UI loop. Return process exit code.</summary>
    int Run(IServiceProvider services, string[] args);
}
