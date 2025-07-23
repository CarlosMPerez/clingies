using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025072302)]
public class _2025072302_SeedDefaultStyle : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertStyle("default", "#FFFFAA",
                "#87CEFA", "Arial", 14, "#000000",
                null, "Arial", 14, "#000000", null);
        Execute.Sql(sql);
    }

    public override void Down()
    {

    }
}
