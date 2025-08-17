using System;
using Avalonia;
using Clingies.ApplicationLogic.Interfaces;

namespace Clingies.Avalonia;

public class AvaloniaFrontendHost : IFrontendHost
{
    public int Run(IServiceProvider services, string[] args)
    {
        BuildAvaloniaApp(services).StartWithClassicDesktopLifetime(args);
        return 0;
    }

    private static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder
            .Configure<App>(() => new App(services))
            .UsePlatformDetect()
            .UseSkia()
            .LogToTrace();
}
