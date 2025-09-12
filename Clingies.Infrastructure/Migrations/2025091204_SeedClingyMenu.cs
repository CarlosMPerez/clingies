using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025091204)]
public class _20250912_04_SeedClingyMenu : Migration
{
    public override void Up()
    {
        // SLEEP
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sleep", "clingy", null, "Sleep...",
                "Sleep the Clingy for a determined interval", true, false, 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("sleep",
            "Assets/menu_light/menu_sleep.png",
            "Assets/menu_dark/menu_sleep.png");
        Execute.Sql(sql);
        // Alarm
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("alarm", "clingy", null, "Alarm...",
                "Set an Alarm on the Clingy", true, false, 40);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("alarm",
            "Assets/menu_light/menu_alarm.png",
            "Assets/menu_dark/menu_alarm.png");
        Execute.Sql(sql);
        // Set title
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("title", "clingy", null, "Set Title...\t\tShift+Ctrl+T",
                "Changes the Title on the Clingy", true, false, 50);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("title",
            "Assets/menu_light/menu_title.png",
            "Assets/menu_dark/menu_title.png");
        Execute.Sql(sql);
        // Lock Content
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("lock", "clingy", null, "Lock",
                "Locks the Clingy contents", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("lock",
            "Assets/menu_light/menu_locked.png",
            "Assets/menu_dark/menu_locked.png");
        Execute.Sql(sql);
        // Unlock Content
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unlock", "clingy", null, "Unlock",
                "Unlocks the Clingy contents", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unlock",
            "Assets/menu_light/menu_unlocked.png",
            "Assets/menu_dark/menu_unlocked.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_locked",
            "Assets/clingy_locked.png",
            "Assets/clingy_locked.png");
        Execute.Sql(sql);
    }
    public override void Down()
    {
        Delete.FromTable("system_menu").Row(new { id = "sleep" });
        Delete.FromTable("system_icon_path").Row(new { id = "sleep" });
        Delete.FromTable("system_menu").Row(new { id = "alarm" });
        Delete.FromTable("system_icon_path").Row(new { id = "alarm" });
        Delete.FromTable("system_menu").Row(new { id = "title" });
        Delete.FromTable("system_icon_path").Row(new { id = "title" });
        Delete.FromTable("system_menu").Row(new { id = "lock" });
        Delete.FromTable("system_icon_path").Row(new { id = "lock" });
        Delete.FromTable("system_menu").Row(new { id = "unlock" });
        Delete.FromTable("system_icon_path").Row(new { id = "unlock" });
    }

}
