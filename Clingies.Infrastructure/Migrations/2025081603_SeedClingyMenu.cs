using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025081603)]
public class _2025081603_SeedClingyMenu : Migration
{
    // COMMENTED ROWS FOR A FUTURE SEED
    public override void Up()
    {
        // SLEEP
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sleep", "clingy", null, "Sleep...",
                "Sleep the Clingy for a determined interval", true, false, 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("sleep",
            "avares://Clingies.Avalonia/Assets/menu_light/menu_sleep.png",
            "avares://Clingies.Avalonia/Assets/menu_dark/menu_sleep.png");
        Execute.Sql(sql);
        // Attach
        // sql = SqlBuilder.BuildInsertSystemTrayMenuItem("attach", "clingy", null, "Attach...",
        //         "???????????", true, false, 20);
        // Execute.Sql(sql);
        // sql = SqlBuilder.BuildInsertSystemTrayIcon("attach",
        //     "avares://Clingies.Avalonia/Assets/menu_light/menu_attach.png",
        //     "avares://Clingies.Avalonia/Assets/menu_dark/menu_attach.png");
        // Execute.Sql(sql);
        // Add to Stack
        // sql = SqlBuilder.BuildInsertSystemTrayMenuItem("add_to_stack", "clingy", null, "Add to Stack",
        //         "Adds the Clingy to an existing Stack", true, false, 30);
        // Execute.Sql(sql);
        // sql = SqlBuilder.BuildInsertSystemTrayIcon("add_to_stack",
        //     "avares://Clingies.Avalonia/Assets/menu_light/menu_stack.png",
        //     "avares://Clingies.Avalonia/Assets/menu_dark/menu_stack.png");
        // Execute.Sql(sql);
        // Alarm
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("alarm", "clingy", null, "Alarm...",
                "Set an Alarm on the Clingy", true, false, 40);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("alarm",
            "avares://Clingies.Avalonia/Assets/menu_light/menu_alarm.png",
            "avares://Clingies.Avalonia/Assets/menu_dark/menu_alarm.png");
        Execute.Sql(sql);
        // Set title
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("title", "clingy", null, "Set Title...\t\tShift+Ctrl+T",
                "Changes the Title on the Clingy", true, false, 50);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("title",
            "avares://Clingies.Avalonia/Assets/menu_light/menu_title.png",
            "avares://Clingies.Avalonia/Assets/menu_dark/menu_title.png");
        Execute.Sql(sql);
        // Set color
        // sql = SqlBuilder.BuildInsertSystemTrayMenuItem("color", "clingy", null, "Set Color...",
        //         "Changes the Color of the Clingy", true, false, 60);
        // Execute.Sql(sql);
        // sql = SqlBuilder.BuildInsertSystemTrayIcon("color",
        //     "avares://Clingies.Avalonia/Assets/menu_light/menu_change_color.png",
        //     "avares://Clingies.Avalonia/Assets/menu_dark/menu_change_color.png");
        // Execute.Sql(sql);
        // Lock Content
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("lock", "clingy", null, "Lock",
                "Locks the Clingy contents", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("lock",
            "avares://Clingies.Avalonia/Assets/menu_light/menu_locked.png",
            "avares://Clingies.Avalonia/Assets/menu_dark/menu_locked.png");
        Execute.Sql(sql);
        // Unlock Content
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unlock", "clingy", null, "Unlock",
                "Unlocks the Clingy contents", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unlock",
            "avares://Clingies.Avalonia/Assets/menu_light/menu_unlocked.png",
            "avares://Clingies.Avalonia/Assets/menu_dark/menu_unlocked.png");
        Execute.Sql(sql);
        // Properties
        // sql = SqlBuilder.BuildInsertSystemTrayMenuItem("properties", "clingy", null, "Properties",
        //         "Sets the Clingy properties", true, false, 80);
        // Execute.Sql(sql);
        // sql = SqlBuilder.BuildInsertSystemTrayIcon("properties",
        //     "avares://Clingies.Avalonia/Assets/menu_light/menu_settings.png",
        //     "avares://Clingies.Avalonia/Assets/menu_dark/menu_settings.png");
        // Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_locked",
            "avares://Clingies.Avalonia/Assets/clingy_locked.png",
            "avares://Clingies.Avalonia/Assets/clingy_locked.png");
        Execute.Sql(sql);
    }
    public override void Down()
    {
        Delete.FromTable("system_menu").Row(new { id = "sleep" });
        Delete.FromTable("system_icon_path").Row(new { id = "sleep" });
        // Delete.FromTable("system_menu").Row(new { id = "attach" });
        // Delete.FromTable("system_icon_path").Row(new { id = "attach" });
        // Delete.FromTable("system_menu").Row(new { id = "add_to_stack" });
        // Delete.FromTable("system_icon_path").Row(new { id = "add_to_stack" });
        Delete.FromTable("system_menu").Row(new { id = "alarm" });
        Delete.FromTable("system_icon_path").Row(new { id = "alarm" });
        Delete.FromTable("system_menu").Row(new { id = "title" });
        Delete.FromTable("system_icon_path").Row(new { id = "title" });
        // Delete.FromTable("system_menu").Row(new { id = "color" });
        // Delete.FromTable("system_icon_path").Row(new { id = "color" });
        Delete.FromTable("system_menu").Row(new { id = "lock" });
        Delete.FromTable("system_icon_path").Row(new { id = "lock" });
        Delete.FromTable("system_menu").Row(new { id = "unlock" });
        Delete.FromTable("system_icon_path").Row(new { id = "unlock" });
        // Delete.FromTable("system_menu").Row(new { id = "properties" });
        // Delete.FromTable("system_icon_path").Row(new { id = "properties" });
    }

}
