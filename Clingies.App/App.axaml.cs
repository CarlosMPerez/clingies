using System;
using System.IO;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Clingies.Infrastructure.Migrations;

namespace Clingies.App;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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


        base.OnFrameworkInitializationCompleted();
    }
}