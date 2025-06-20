using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Clingies.Infrastructure.Migrations;

namespace Clingies.App;

public partial class App : Avalonia.Application
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
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _trayIcon = DrawTrayIcon();
            RunMigrations();
        }


        base.OnFrameworkInitializationCompleted();
    }

    private void OnNewClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
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
        var trayIcon = new TrayIcon
        {
            Icon = new WindowIcon("Assets/icon-app-clingy.png"),
            ToolTipText = "Clingies"
        };

        trayIcon.Menu = new NativeMenu();
        var newItem = new NativeMenuItem("New");
        newItem.Click += OnNewClick;
        var settingsItem = new NativeMenuItem("Settings");
        newItem.Click += OnSettingsClick;
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