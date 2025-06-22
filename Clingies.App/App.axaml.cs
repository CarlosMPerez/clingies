using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Clingies.Application.Services;
using Clingies.Domain.Factories;
using Clingies.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies.App;

public partial class App(IServiceProvider services) : Avalonia.Application
{
    private TrayIcon? _trayIcon;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _trayIcon = DrawTrayIcon();
            RunMigrations();
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void OnNewClickAsync(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var service = services.GetRequiredService<ClingyService>();
            var clingy = await service.CreateAsync("", "");
            var window = new Windows.ClingyWindow();
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
    private TrayIcon DrawTrayIcon()
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
        newItem.Click += OnNewClickAsync;
        var settingsItem = new NativeMenuItem("Settings");
        settingsItem.Click += OnSettingsClick;
        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += OnExitClick;

        trayIcon.Menu.Items.Add(newItem);
        trayIcon.Menu.Items.Add(settingsItem);
        trayIcon.Menu.Items.Add(new NativeMenuItemSeparator { Header = "-" });
        trayIcon.Menu.Items.Add(exitItem);

        trayIcon.IsVisible = true;

        return trayIcon;
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
}