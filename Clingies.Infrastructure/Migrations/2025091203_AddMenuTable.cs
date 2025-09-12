using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025091203)]
public class _20250912_03_AddMenuTable : Migration
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

        if (!Schema.Table("app_icon_paths").Exists())
        {
            Create.Table("app_icon_paths")
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
        if (Schema.Table("app_icon_paths").Exists())
        {
            Delete.Table("app_icon_paths");
        }
    }
}
