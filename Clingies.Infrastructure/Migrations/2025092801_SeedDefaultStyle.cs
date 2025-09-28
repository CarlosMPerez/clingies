using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025092801)]
public class _2025092801_SeedDefaultStyle : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertStyle("System", "#FFFFB8", "monospace",
                                14, "#000000", "0000", true, true);
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("styles").Row(new { style_name = "System" });
    }
}
