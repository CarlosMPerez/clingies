using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025071801)]
public class _2025071801_AddMenuTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("system_menu").Exists())
        {
            Create.Table("system_menu")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("menu_type").AsString().Nullable()
                .WithColumn("parent_id").AsString().Nullable()
                .WithColumn("label").AsString().Nullable()
                .WithColumn("tooltip").AsString().Nullable()
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("separator").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("sort_order").AsInt32().WithDefaultValue(0);
        }

        if (!Schema.Table("system_icon_path").Exists())
        {
            Create.Table("system_icon_path")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("light_path").AsString().NotNullable()
                .WithColumn("dark_path").AsString().NotNullable();
        }
    }

    public override void Down()
    {
        if (Schema.Table("system_menu").Exists())
        {
            Delete.Table("system_menu");
        }
        if (Schema.Table("system_icon_path").Exists())
        {
            Delete.Table("system_icon_path");
        }
    }
}
