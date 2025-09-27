using System.Data;
using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025092701)]
public class _20250927_01_AddStylesFeature : Migration
{
    public override void Up()
    {
        if (!Schema.Table("styles").Exists())
        {
            Create.Table("styles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("body_color").AsString().NotNullable()
                .WithColumn("body_font").AsString().NotNullable()
                .WithColumn("body_font_color").AsString().NotNullable()
                .WithColumn("body_font_size").AsInt32().NotNullable()
                .WithColumn("body_font_decorations").AsString().Nullable()
                .WithColumn("is_default").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }

    public override void Down()
    {
        if (Schema.Table("styles").Exists()) Delete.Table("styles");
    }
}
