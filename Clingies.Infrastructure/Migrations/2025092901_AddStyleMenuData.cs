using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025092901)]
public class _2025092901_AddStyleMenuData : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("style_manager", "tray", null, "Style Manager...",
                "Manage Clingy styles", true, false, 71);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("style_manager",
            "Assets/menu_light/menu_change_color.png",
            "Assets/menu_dark/menu_change_color.png");
        Execute.Sql(sql);

        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("set_style", "clingy", null, "Set to Style",
                "Sets the Clingy Style", true, false, 80);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("set_style",
            "Assets/menu_light/menu_change_color.png",
            "Assets/menu_dark/menu_change_color.png");
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_menu").Row(new { id = "style_manager" });
        Delete.FromTable("system_icon_path").Row(new { id = "style_manager" });
        Delete.FromTable("system_menu").Row(new { id = "set_style" });
        Delete.FromTable("system_icon_path").Row(new { id = "set_style" });
    }
}
