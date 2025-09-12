using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Clingies.Infrastructure.Migrations
{
    public class MigrationRunnerService(string dbPath)
    {
        private readonly string connectionString = $"Data Source={dbPath}";
        public void MigrateUp()
        {
            var serviceProvider = CreateServices();
            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        private IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(_20250912_01_Init_Clingies_Schema).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);
        }
    }
}
