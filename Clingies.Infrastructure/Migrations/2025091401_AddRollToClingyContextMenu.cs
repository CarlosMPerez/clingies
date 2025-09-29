using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025091401)]
public class _20250914_01_AddRollToClingyContextMenu : Migration
{
    public override void Up()
    {
        // ROLL UP
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rollup", "clingy", null, "Roll Up",
                "Rolls up the Clingy content", true, false, 60);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rollup",
            "Assets/menu_light/menu_rollup.png",
            "Assets/menu_dark/menu_rollup.png");
        Execute.Sql(sql);
        // ROLL DOWN
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rolldown", "clingy", null, "Roll Down",
                "Rolls down the Clingy content", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rolldown",
            "Assets/menu_light/menu_rolldown.png",
            "Assets/menu_dark/menu_rolldown.png");
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_menu").Row(new { id = "rollup" });
        Delete.FromTable("system_icon_path").Row(new { id = "rollup" });
        Delete.FromTable("system_menu").Row(new { id = "rolldown" });
        Delete.FromTable("system_icon_path").Row(new { id = "rolldown" });

    }
}
