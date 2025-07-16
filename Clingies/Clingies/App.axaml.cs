using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Clingies.Common;
using Clingies.Factories;
using Clingies.Infrastructure.Migrations;
using Clingies.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies;

public partial class App : Application
{
    public IServiceProvider _services;
    private ClingyWindowFactory _windowFactory;
    private IClingiesLogger _logger;

    [Obsolete("Used only by Avalonia XAML loader", true)]
    public App() { }

    public App(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _windowFactory = _services.GetRequiredService<ClingyWindowFactory>();
        _logger = _services.GetRequiredService<IClingiesLogger>();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ClingyWindow();
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            DrawTrayIcon();
            RunMigrations();
            _windowFactory.InitActiveWindows();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnNewClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var defClingyWidth = 300;
            var defClingyHeight = 100;

            var lifetime = App.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            Window? anchor = lifetime?.Windows.Count > 0 ? lifetime.Windows[0] : null;
            var screen = anchor?.Screens.ScreenFromVisual(anchor)?.WorkingArea
                                  ?? new PixelRect(0, 0, 800, 600);
            var centerX = screen.X + (screen.Width - defClingyWidth) / 2;
            var centerY = screen.Y + (screen.Height - defClingyHeight) / 2;

            _windowFactory.CreateNewWindow(centerX, centerY);
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

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        _windowFactory.RenderAllWindows();
    }

    private void DrawTrayIcon()
    {
        string embeddedPath = "avares://Clingies/Assets/icon-app-clingy.png";
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

        trayIcon.Clicked += OnTrayIconClicked;
        trayIcon.IsVisible = true;
    }

    private void RunMigrations()
    {
        _logger.Info("Running migrations");
        // Path to your SQLite DB file, e.g., in user app data folder
        var dbPath = Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Clingies", "clingies.db");

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        // Run migrations
        var migrator = new MigrationRunnerService(dbPath);
        migrator.MigrateUp();
        _logger.Info("Migrations done");
    }
}