﻿using Avalonia;
using System;
using System.IO;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Interfaces;
using Clingies.Factories;
using Clingies.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Providers;
using Clingies.Services;

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
            services.AddSingleton<IConnectionFactory>(sp =>
            {
                var logger = sp.GetRequiredService<IClingiesLogger>();
                return new ConnectionFactory(dbPath, logger);
            });
            services.AddSingleton<IClingyRepository, ClingyRepository>();
            services.AddSingleton<IMenuRepository, MenuRepository>();
            services.AddSingleton<IStyleRepository, StyleRepository>();
            services.AddSingleton<IIconPathRepository, IconPathRepository>();
            services.AddSingleton<ITrayCommandProvider, TrayCommandProvider>();
            services.AddSingleton<MenuFactory>();
            services.AddSingleton<ClingyWindowFactory>();
            services.AddSingleton<ClingyService>();
            services.AddSingleton<StyleService>();
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

    private static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder
            .Configure<App>(() => new App(services))
            .UsePlatformDetect()
            .UseSkia()
            .LogToTrace();
}

