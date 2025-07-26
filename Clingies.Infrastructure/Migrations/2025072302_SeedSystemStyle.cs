using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025072302)]
public class _2025072302_SeedSystemStyle : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertStyle("system", "#FFFFB4",
                "#87CEFA", "Arial", 14, "#000000",
                null, "Arial", 14, "#000000", null, true, true);
        Execute.Sql(sql);
    }

    public override void Down()
    {

    }
}
