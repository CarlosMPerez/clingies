using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025072301)]
public class _2025072301_AddStyling : Migration
{
    public override void Up()
    {
        if (!Schema.Table("styles").Exists())
        {
            Create.Table("styles")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("body_color").AsString().NotNullable()
                .WithColumn("title_color").AsString().NotNullable()
                .WithColumn("body_font").AsString().NotNullable()
                .WithColumn("body_font_color").AsString().NotNullable()
                .WithColumn("body_font_size").AsInt32().NotNullable()
                .WithColumn("body_font_decorations").AsString().Nullable()
                .WithColumn("title_font").AsString().NotNullable()
                .WithColumn("title_font_size").AsInt32().Nullable()
                .WithColumn("title_font_color").AsString().NotNullable()
                .WithColumn("title_font_decorations").AsString().Nullable()
                .WithColumn("is_default").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }

    public override void Down()
    {
        if (Schema.Table("styles").Exists()) Delete.Table("styles");
    }
}
