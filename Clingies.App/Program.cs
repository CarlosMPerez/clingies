using System;
using Avalonia;
using Clingies.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add DI bindings here
                // services.AddSingleton<IMyService, MyService>();
            })
            .Build();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .UseSkia()
               .LogToTrace();

}

