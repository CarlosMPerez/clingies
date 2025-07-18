using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025071801)]
public class _2025071801_AddMenuTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("system_tray_menu").Exists())
        {
            Create.Table("system_tray_menu")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("label").AsString().Nullable()
                .WithColumn("tooltip").AsString().Nullable()
                .WithColumn("icon").AsString().Nullable()
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("separator").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("parent_id").AsString().Nullable()
                .WithColumn("sort_order").AsInt32().WithDefaultValue(0);
        }
    }

    public override void Down()
    {
        if (Schema.Table("system_tray_menu").Exists())
        {
            Delete.Table("system_tray_menu");
        }
    }
}
