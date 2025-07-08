using System;
using System.IO;
using Avalonia;
using Clingies.App;
using Clingies.App.Common;
using Clingies.App.Factories;
using Clingies.Application.Services;
using Clingies.Common;
using Clingies.Domain.Interfaces;
using Clingies.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logPath = Path.Combine(Path.Combine(AppContext.BaseDirectory, "logs"), "clingies.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();
        try
        {
            Log.Information("Application started");
            var services = ConfigureServices();
            BuildAvaloniaApp(services).StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.Information("Application finished");
            Log.CloseAndFlush();
        }
    }

    private static IServiceProvider ConfigureServices()
    {
		try
		{
			var services = new ServiceCollection();
			// TODO: Change this for a configuration file so user can decide where the db is
			var dbPath = Path.Combine(Environment
					.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"Clingies", "clingies.db");
			services.AddSingleton<IClingiesLogger, ClingiesLogger>();
			Log.Information("Registering connection for db at {0}", dbPath);
			services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory(dbPath));
			services.AddSingleton<IClingyRepository, ClingyRepository>();
			services.AddSingleton<ClingyWindowFactory>();
			services.AddSingleton<ClingyService>();

			return services.BuildServiceProvider();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error registering services");
			throw;
		}
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder
            .Configure(() => new App(services))
            .UsePlatformDetect()
            .UseSkia()
            .LogToTrace();
}

