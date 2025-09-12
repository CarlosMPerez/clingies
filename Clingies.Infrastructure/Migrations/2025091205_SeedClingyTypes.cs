using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025091205)]

public class _20250912_05_SeedClingyTypes : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertClingyType(1, "desktop");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertClingyType(2, "sleeping");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertClingyType(3, "recurring");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertClingyType(4, "closed");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertClingyType(5, "stored");
        Execute.Sql(sql);
    }
    public override void Down()
    {
        Delete.FromTable("clingy_types").Row(new { id = 5 });
        Delete.FromTable("clingy_types").Row(new { id = 4 });
        Delete.FromTable("clingy_types").Row(new { id = 3 });
        Delete.FromTable("clingy_types").Row(new { id = 2 });
        Delete.FromTable("clingy_types").Row(new { id = 1 });
    }
}
