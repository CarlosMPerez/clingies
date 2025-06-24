using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Clingies.App.Windows;
using Clingies.Application.Services;
using Clingies.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies.App;

public partial class App : Avalonia.Application
{
    public IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DrawTrayIcon();
            RunMigrations();
            RenderActiveClingies();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnNewClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var service = _services.GetRequiredService<ClingyService>();
            var clingy = service.Create("", "");
            var window = new Windows.ClingyWindow(service, clingy);
            window.Show();
        }
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        Console.WriteLine("Settings clicked");
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
    private void DrawTrayIcon()
    {
        string embeddedPath = "avares://Clingies.App/Assets/icon-app-clingy.png";
        var uri = new Uri(embeddedPath);
        using var stream = AssetLoader.Open(uri);

        var trayIcon = new TrayIcon
        {
            Icon = new WindowIcon(stream),
            ToolTipText = "Clingies"
        };

        trayIcon.Menu = new NativeMenu();
        var newItem = new NativeMenuItem("New");
        newItem.Click += OnNewClick;
        var settingsItem = new NativeMenuItem("Settings");
        settingsItem.Click += OnSettingsClick;
        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += OnExitClick;

        trayIcon.Menu.Items.Add(newItem);
        trayIcon.Menu.Items.Add(settingsItem);
        trayIcon.Menu.Items.Add(new NativeMenuItemSeparator { Header = "-" });
        trayIcon.Menu.Items.Add(exitItem);

        trayIcon.IsVisible = true;
    }

    private void RunMigrations()
    {
        // Path to your SQLite DB file, e.g., in user app data folder
        var dbPath = Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Clingies", "clingies.db");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Run migrations
        var migrator = new MigrationRunnerService(dbPath);
        migrator.MigrateUp();
    }

    private void RenderActiveClingies()
    {
        var service = _services.GetRequiredService<ClingyService>();
        foreach (var clingy in service.GetAllActive())
        {
            var window = new ClingyWindow(service, clingy);
            window.Show();
        }
    }
}