using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Interfaces;
using Clingies.Infrastructure.Data;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Providers;
using Clingies.GtkFront;
using Clingies.GtkFront.Utils;
using Clingies.GtkFront.Factories;
using Clingies.Infrastructure.Migrations;
using Gtk;

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
            var services = new ServiceCollection();
            ConfigureServices(services);
            var sp = services.BuildServiceProvider();

            RunMigrations(sp);

            Application.Init();
            GtkFrontendHost host = new GtkFrontendHost(
                sp.GetRequiredService<IClingiesLogger>(),
                sp.GetRequiredService<IIconPathRepository>(),
                sp.GetRequiredService<ClingyWindowManager>(),
                sp.GetRequiredService<MenuFactory>(),
                sp.GetRequiredService<UtilsService>());
            host.Run();
            Application.Run();
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

    private static void ConfigureServices(ServiceCollection services)
    {
        try
        {
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
            services.AddSingleton<ClingyWindowManager>();
            services.AddSingleton<ClingyService>();
            services.AddSingleton<MenuService>();
            services.AddSingleton<ITrayCommandProvider, TrayCommandProvider>();
            services.AddSingleton<ITrayCommandController, TrayCommandController>();
            services.AddSingleton<Func<IContextCommandController, IContextCommandProvider>>(sp =>
                                    controller => new ContextCommandProvider(controller));
            services.AddSingleton<UtilsService>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering services");
            throw;
        }
    }

    private static void RunMigrations(IServiceProvider sp)
    {
        var logger = sp.GetRequiredService<IClingiesLogger>();
        logger.Info("Running migrations");

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Clingies", "clingies.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var migrator = new MigrationRunnerService(dbPath);
        migrator.MigrateUp();

        logger.Info("Migrations done");
    }
}

