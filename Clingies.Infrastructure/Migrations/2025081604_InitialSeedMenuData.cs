using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025081604)]
public class _2025081604_InitialSeedMenuData : Migration
{
    public override void Up()
    {
        // COMMENTED ROWS RESERVED FOR THE FUTURE

        // NEW
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("new", "tray", null, "New",
                "Create a new Clingy", true, false, 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("new",
            "Assets/menu_light/menu_new.png",
            "Assets/menu_dark/menu_new.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep001", "tray", null, null, null, true, true, 30);
        Execute.Sql(sql);
        // SET ALL
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("set_all", "tray", null, "Set all",
            "Change state to all Clingies", true, false, 40);
        Execute.Sql(sql);
        // SET ALL __> ROLLED UP
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rolled_up", "tray", "set_all", "Rolled Up",
            "Roll up all Clingies", true, false, 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rolled_up",
            "Assets/menu_light/menu_rollup.png",
            "Assets/menu_dark/menu_rollup.png");
        Execute.Sql(sql);
        // SET ALL __> ROLLED DOWN
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rolled_down", "tray", "set_all", "Rolled Down",
            "Roll down all Clingies", true, false, 20);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rolled_down",
            "Assets/menu_light/menu_rolldown.png",
            "Assets/menu_dark/menu_rolldown.png");
        Execute.Sql(sql);
        // SET ALL __> PINNED
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("pinned", "tray", "set_all", "Pinned",
            "Pin all Clingies", true, false, 30);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("pinned",
            "Assets/menu_light/menu_pinned.png",
            "Assets/menu_dark/menu_pinned.png");
        Execute.Sql(sql);
        // SET ALL __> UNPINNED
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unpinned", "tray", "set_all", "Unpinned",
            "Unpin all Clingies", true, false, 40);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unpinned",
            "Assets/menu_light/menu_unpinned.png",
            "Assets/menu_dark/menu_unpinned.png");
        Execute.Sql(sql);
        // SET ALL __> LOCK
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("locked", "tray", "set_all", "Locked",
            "Lock all Clingies", true, false, 50);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("locked",
            "Assets/menu_light/menu_locked.png",
            "Assets/menu_dark/menu_locked.png");
        Execute.Sql(sql);
        // SET ALL __> UNLOCK
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unlocked", "tray", "set_all", "Unlocked",
            "Unlock all Clingies", true, false, 60);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unlocked",
            "Assets/menu_light/menu_unlocked.png",
            "Assets/menu_dark/menu_unlocked.png");
        Execute.Sql(sql);
        // SET ALL _> VISIBLE
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("show", "tray", "set_all", "Show all",
            "Show all Clingies", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("show",
            "Assets/menu_light/menu_show.png",
            "Assets/menu_dark/menu_show.png");
        Execute.Sql(sql);
        // SET ALL _> INVISIBLE
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("hide", "tray", "set_all", "Hide",
            "Hide all Clingies", true, false, 80);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("hide",
            "Assets/menu_light/menu_hide.png",
            "Assets/menu_dark/menu_hide.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep002", "tray", null, null, null, true, true, 50);
        Execute.Sql(sql);
        // MANAGE NOTES
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("manage_clingies", "tray", null, "Manage Clingies...",
            "Manage Clingies", true, false, 60);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("manage_clingies",
            "Assets/menu_light/menu_manage.png",
            "Assets/menu_dark/menu_manage.png");
        Execute.Sql(sql);
        // SETTINGS
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("settings", "tray", null, "Settings...",
            "Access the Settings window", true, false, 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("settings",
            "Assets/menu_light/menu_settings.png",
            "Assets/menu_dark/menu_settings.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep003", "tray", null, null, null, true, true, 80);
        Execute.Sql(sql);
        // HELP
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("help", "tray", null, "Clingies help...",
            "Clingies Help", true, false, 90);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("help",
            "Assets/menu_light/menu_help.png",
            "Assets/menu_dark/menu_help.png");
        Execute.Sql(sql);
        // ABOUT
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("about", "tray", null, "About Clingies...",
            "About the Application", true, false, 100);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("about",
            "Assets/menu_light/menu_info.png",
            "Assets/menu_dark/menu_info.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep004", "tray", null, null, null, true, true, 110);
        Execute.Sql(sql);
        // EXIT
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("exit", "tray", null, "Exit",
            "Close the application", true, false, 120);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("exit",
            "Assets/menu_light/menu_exit.png",
            "Assets/menu_dark/menu_exit.png");
        Execute.Sql(sql);

        // Besides the menus, we insert the resource paths for the WINDOW icons
        // They don't have light_dark versions so we store the same path twice
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_close",
            "Assets/clingy_close.png",
            "Assets/clingy_close.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_pinned",
            "Assets/clingy_pinned.png",
            "Assets/clingy_pinned.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_unpinned",
            "Assets/clingy_unpinned.png",
            "Assets/clingy_unpinned.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_icon",
            "Assets/clingy_icon_light.png",
            "Assets/clingy_icon_dark.png");
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_menu").Row(new { id = "new" });
        Delete.FromTable("system_icon_path").Row(new { id = "new" });
        Delete.FromTable("system_menu").Row(new { id = "new_stack" });
        Delete.FromTable("system_icon_path").Row(new { id = "new_stack" });
        Delete.FromTable("system_menu").Row(new { id = "sep001" });
        Delete.FromTable("system_menu").Row(new { id = "set_all" });
        Delete.FromTable("system_menu").Row(new { id = "rolled_up" });
        Delete.FromTable("system_icon_path").Row(new { id = "rolled_up" });
        Delete.FromTable("system_menu").Row(new { id = "rolled_down" });
        Delete.FromTable("system_icon_path").Row(new { id = "rolled_down" });
        Delete.FromTable("system_menu").Row(new { id = "pinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "pinned" });
        Delete.FromTable("system_menu").Row(new { id = "unpinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "unpinned" });
        Delete.FromTable("system_menu").Row(new { id = "locked" });
        Delete.FromTable("system_icon_path").Row(new { id = "locked" });
        Delete.FromTable("system_menu").Row(new { id = "unlocked" });
        Delete.FromTable("system_icon_path").Row(new { id = "unlocked" });
        Delete.FromTable("system_menu").Row(new { id = "show" });
        Delete.FromTable("system_icon_path").Row(new { id = "show" });
        Delete.FromTable("system_menu").Row(new { id = "hide" });
        Delete.FromTable("system_icon_path").Row(new { id = "hide" });
        Delete.FromTable("system_menu").Row(new { id = "sep002" });
        Delete.FromTable("system_menu").Row(new { id = "manage_clingies" });
        Delete.FromTable("system_icon_path").Row(new { id = "manage_clingies" });
        Delete.FromTable("system_menu").Row(new { id = "settings" });
        Delete.FromTable("system_icon_path").Row(new { id = "settings" });
        Delete.FromTable("system_menu").Row(new { id = "sep003" });
        Delete.FromTable("system_menu").Row(new { id = "help" });
        Delete.FromTable("system_icon_path").Row(new { id = "help" });
        Delete.FromTable("system_menu").Row(new { id = "about" });
        Delete.FromTable("system_icon_path").Row(new { id = "about" });
        Delete.FromTable("system_menu").Row(new { id = "sep004" });
        Delete.FromTable("system_menu").Row(new { id = "exit" });
        Delete.FromTable("system_icon_path").Row(new { id = "exit" });

        Delete.FromTable("system_icon_path").Row(new { id = "clingy_close" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy_pinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy_unpinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy_icon" });
    }
}
