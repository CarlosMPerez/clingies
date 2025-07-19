using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Factories;
using Clingies.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies;

public partial class App : Application, IClingiesCommandController
{
    private IServiceProvider _services;
    private ClingyWindowFactory _windowFactory;
    private TrayMenuFactory _trayMenuFactory;
    private IClingiesLogger _logger;
    private IIconPathRepository _iconRepo;
    private IClassicDesktopStyleApplicationLifetime _desktop;
    public static IServiceProvider Services { get; private set; }

    public App(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Services = _services;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _windowFactory = _services.GetRequiredService<ClingyWindowFactory>();
            _trayMenuFactory = _services.GetRequiredService<TrayMenuFactory>();
            _logger = _services.GetRequiredService<IClingiesLogger>();
            _iconRepo = _services.GetRequiredService<IIconPathRepository>();
            _desktop = desktop;           
            _desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            RunMigrations();
            DrawTrayIcon();
            _windowFactory.InitActiveWindows();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void OnNewClick(object? sender, EventArgs e)
    {
        if (_desktop is not null)
        {
            var defClingyWidth = 300;
            var defClingyHeight = 100;

            var screen = GetDesktopWorkingArea();
            var centerX = screen.X + (screen.Width - defClingyWidth) / 2;
            var centerY = screen.Y + (screen.Height - defClingyHeight) / 2;

            _windowFactory.CreateNewWindow(centerX, centerY);
        }
    }

    public void OnSettingsClick(object? sender, EventArgs e)
    {
        Console.WriteLine("Settings clicked");
    }

    public void OnExitClick(object? sender, EventArgs e)
    {
        if (_desktop is not null)
        {
            _desktop.Shutdown();
        }
    }

    private PixelRect GetDesktopWorkingArea()
    {
        if (_desktop.Windows.Count > 0)
        {
            var anchor = _desktop.Windows[0];
            return anchor.Screens.ScreenFromVisual(anchor)?.WorkingArea
                ?? new PixelRect(0, 0, 800, 600);
        }

        var probe = new Window
        {
            Width = 1,
            Height = 1,
            SystemDecorations = SystemDecorations.None,
            CanResize = false,
            ShowInTaskbar = false,
            IsVisible = false
        };

        probe.Show(); // required to attach to a screen
        var screen = probe.Screens.ScreenFromVisual(probe)?.WorkingArea
                    ?? new PixelRect(0, 0, 800, 600);
        probe.Close();

        return screen;
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        _windowFactory.RenderAllWindows();
    }

    private void DrawTrayIcon()
    {
        string? iconPath = _iconRepo.GetLightPath("clingy-icon");
        if (!string.IsNullOrEmpty(iconPath))
        {
            var uri = new Uri(iconPath!);
            using var stream = AssetLoader.Open(uri);

            var trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(stream),
                ToolTipText = "Clingies",
                Menu = _trayMenuFactory.BuildTrayMenu(),
                IsVisible = true
            };

            trayIcon.Clicked += OnTrayIconClicked;
        }
        else _logger.Error(new Exception(), "Could not find main tray icon in DB");
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

    // IClingiesCommandController implementation, linked those commands with these events
    public void CreateNewClingy()
    {
        OnNewClick(null, EventArgs.Empty);
    }

    public void ShowSettings()
    {
        OnSettingsClick(null, EventArgs.Empty);
    }

    public void ExitApp()
    {
        OnExitClick(null, EventArgs.Empty);
    }    
}