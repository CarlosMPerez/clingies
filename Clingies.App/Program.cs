using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Clingies.App;
using Clingies.Application.Services;
using Clingies.Domain.Interfaces;
using Clingies.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var services = ConfigureServices();
        var app = new App(services);

        BuildAvaloniaApp(app).StartWithClassicDesktopLifetime(args);
    }
    
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // TODO: Change this for a configuration file so user can decide where the db is
        var dbPath = Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Clingies", "clingies.db");
        services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory(dbPath));
        services.AddSingleton<IClingyRepository, ClingyRepository>();
        services.AddSingleton<ClingyService>();

        return services.BuildServiceProvider();
    }    

    public static AppBuilder BuildAvaloniaApp(App app)
        => AppBuilder
            .Configure(() => app)
            .UsePlatformDetect()
            .UseSkia()
            .UseX11()
            .LogToTrace();
}

