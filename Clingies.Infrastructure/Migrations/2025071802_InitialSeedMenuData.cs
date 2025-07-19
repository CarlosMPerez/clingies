using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025071802)]
public class _2025071802_InitialSeedMenuData : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertSystemTrayItemSql("new", "New", "Create a new Clingy", "avares://Clingies/Assets/menu-new.png", 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayItemSql("settings", "Settings", "Access the Settings window", "avares://Clingies/Assets/menu-settings.png", 20);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayItemSql("sep1", null, null, null, 30, true, true, null);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayItemSql("exit", "Exit", "Close the application", "avares://Clingies/Assets/menu-close.png", 40);
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_tray_menu").Row(new { id = "new" });
        Delete.FromTable("system_tray_menu").Row(new { id = "settings" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep1" });
        Delete.FromTable("system_tray_menu").Row(new { id = "exit" });
    }
}
