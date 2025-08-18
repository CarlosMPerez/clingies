using Avalonia;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Interfaces;
using Clingies.Avalonia.Factories;
using Clingies.Infrastructure.Data;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Providers;
using Clingies.Gtk;
using Clingies.Avalonia;
using Clingies.Avalonia.Services;

namespace Clingies;

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
            var mode = (args.Length > 0 ? args[0] : null) ?? "gtk"; // TO BE OBTAINED FROM SETTINGS?

            IFrontendHost host = mode switch
            {
                "gtk" => new GtkFrontendHost(),
                "avalonia" => new AvaloniaFrontendHost(),
                _ => new GtkFrontendHost()
            };

            host.Run(services, args);

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
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var logger = sp.GetRequiredService<IClingiesLogger>();
                return new ConnectionFactory(dbPath, logger);
            });
            services.AddSingleton<IClingyRepository, ClingyRepository>();
            services.AddSingleton<IMenuRepository, MenuRepository>();
            services.AddSingleton<IIconPathRepository, IconPathRepository>();
            services.AddSingleton<ITrayCommandProvider, TrayCommandProvider>();
            services.AddSingleton<MenuFactory>();
            services.AddSingleton<ClingyWindowFactory>();
            services.AddSingleton<ClingyService>();
            services.AddSingleton(sp => (ITrayCommandController)Application.Current!);
            services.AddSingleton<Func<IContextCommandController, IContextCommandProvider>>(sp =>
                                    controller => new ContextCommandProvider(controller));
            services.AddSingleton<UtilsService>();

            return services.BuildServiceProvider();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering services");
            throw;
        }
    }
}

