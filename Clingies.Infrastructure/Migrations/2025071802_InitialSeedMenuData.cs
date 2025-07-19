using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025071802)]
public class _2025071802_InitialSeedMenuData : Migration
{
    public override void Up()
    {
        // NEW
        string sql = SqlBuilder.BuildInsertSystemTrayMenuItem("new", "New",
                "Create a new Clingy", 10);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("new",
            "avares://Clingies/Assets/menu-light/menu-new.png",
            "avares://Clingies/Assets/menu-dark/menu-new.png");
        Execute.Sql(sql);
        // NEW STACK
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("new_stack", "New Stack",
            "New Clingies Stack", 20);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("new_stack",
            "avares://Clingies/Assets/menu-light/menu-stack.png",
            "avares://Clingies/Assets/menu-dark/menu-stack.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep001", null, null, 30, false, true);
        Execute.Sql(sql);
        // SET ALL
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("set_all", "Set all",
            "Change state to all Clingies", 40);
        Execute.Sql(sql);
        // SET ALL --> ROLLED UP
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rolled_up", "Rolled Up",
            "Roll up all Clingies", 10, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rolled_up",
            "avares://Clingies/Assets/menu-light/menu-rollup.png",
            "avares://Clingies/Assets/menu-dark/menu-rollup.png");
        Execute.Sql(sql);
        // SET ALL --> ROLLED DOWN
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("rolled_down", "Rolled Down",
            "Roll down all Clingies", 20, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("rolled_down",
            "avares://Clingies/Assets/menu-light/menu-rolldown.png",
            "avares://Clingies/Assets/menu-dark/menu-rolldown.png");
        Execute.Sql(sql);
        // SET ALL --> PINNED
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("pinned", "Pinned",
            "Pin all Clingies", 30, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("pinned",
            "avares://Clingies/Assets/menu-light/menu-pinned.png",
            "avares://Clingies/Assets/menu-dark/menu-pinned.png");
        Execute.Sql(sql);
        // SET ALL --> UNPINNED
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unpinned", "Unpinned",
            "Unpin all Clingies", 40, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unpinned",
            "avares://Clingies/Assets/menu-light/menu-unpinned.png",
            "avares://Clingies/Assets/menu-dark/menu-unpinned.png");
        Execute.Sql(sql);
        // SET ALL --> LOCK
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("locked", "Locked",
            "Lock all Clingies", 50, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("locked",
            "avares://Clingies/Assets/menu-light/menu-locked.png",
            "avares://Clingies/Assets/menu-dark/menu-locked.png");
        Execute.Sql(sql);
        // SET ALL --> UNLOCK
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("unlocked", "Unlocked",
            "Unlock all Clingies", 60, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("unlocked",
            "avares://Clingies/Assets/menu-light/menu-unlocked.png",
            "avares://Clingies/Assets/menu-dark/menu-unlocked.png");
        Execute.Sql(sql);
        // SET ALL -> VISIBLE
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("show", "Show all",
            "Show all Clingies", 70, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("show",
            "avares://Clingies/Assets/menu-light/menu-show.png",
            "avares://Clingies/Assets/menu-dark/menu-show.png");
        Execute.Sql(sql);
        // SET ALL -> UNVISIBLE
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("hide", "Hide",
            "Hide all Clingies", 80, true, false, "set_all");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("hide",
            "avares://Clingies/Assets/menu-light/menu-hide.png",
            "avares://Clingies/Assets/menu-dark/menu-hide.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep002", null, null, 50, false, true);
        Execute.Sql(sql);
        // MANAGE NOTES
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("manage_clingies", "Manage Clingies...",
            "Manage Clingies", 60);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("manage_clingies",
            "avares://Clingies/Assets/menu-light/menu-manage.png",
            "avares://Clingies/Assets/menu-dark/menu-manage.png");
        Execute.Sql(sql);
        // SETTINGS
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("settings", "Settings...",
            "Access the Settings window", 70);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("settings",
            "avares://Clingies/Assets/menu-light/menu-settings.png",
            "avares://Clingies/Assets/menu-dark/menu-settings.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep003", null, null, 80, false, true);
        Execute.Sql(sql);
        // HELP
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("help", "Clingies help...",
            "Clingies Help", 90);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("help",
            "avares://Clingies/Assets/menu-light/menu-help.png",
            "avares://Clingies/Assets/menu-dark/menu-help.png");
        Execute.Sql(sql);
        // ABOUT
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("about", "About Clingies...",
            "About the Application", 100);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("about",
            "avares://Clingies/Assets/menu-light/menu-info.png",
            "avares://Clingies/Assets/menu-dark/menu-info.png");
        Execute.Sql(sql);
        // SEPARATOR
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("sep004", null, null, 110, false, true);
        Execute.Sql(sql);
        // EXIT
        sql = SqlBuilder.BuildInsertSystemTrayMenuItem("exit", "Exit",
            "Close the application", 120);
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("exit",
            "avares://Clingies/Assets/menu-light/menu-exit.png",
            "avares://Clingies/Assets/menu-dark/menu-exit.png");
        Execute.Sql(sql);

        // Besides the menus, we insert the resource paths for the WINDOW icons
        // They don't have light-dark versions so we store the same path twice
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy-close",
            "avares://Clingies/Assets/clingy-close.png",
            "avares://Clingies/Assets/clingy-close.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy-pinned",
            "avares://Clingies/Assets/clingy-pinned.png",
            "avares://Clingies/Assets/clingy-pinned.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy-unpinned",
            "avares://Clingies/Assets/clingy-unpinned.png",
            "avares://Clingies/Assets/clingy-unpinned.png");
        Execute.Sql(sql);
        sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy-icon",
            "avares://Clingies/Assets/clingy-icon-light.png",
            "avares://Clingies/Assets/clingy-icon-dark.png");
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_tray_menu").Row(new { id = "new" });
        Delete.FromTable("system_tray_menu").Row(new { id = "new_stack" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep001" });
        Delete.FromTable("system_tray_menu").Row(new { id = "set_all" });
        Delete.FromTable("system_tray_menu").Row(new { id = "rolled_up" });
        Delete.FromTable("system_tray_menu").Row(new { id = "rolled_down" });
        Delete.FromTable("system_tray_menu").Row(new { id = "pinned" });
        Delete.FromTable("system_tray_menu").Row(new { id = "unpinned" });
        Delete.FromTable("system_tray_menu").Row(new { id = "locked" });
        Delete.FromTable("system_tray_menu").Row(new { id = "unlocked" });
        Delete.FromTable("system_tray_menu").Row(new { id = "show" });
        Delete.FromTable("system_tray_menu").Row(new { id = "hide" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep002" });
        Delete.FromTable("system_tray_menu").Row(new { id = "manage_clingies" });
        Delete.FromTable("system_tray_menu").Row(new { id = "settings" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep003" });
        Delete.FromTable("system_tray_menu").Row(new { id = "help" });
        Delete.FromTable("system_tray_menu").Row(new { id = "about" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep004" });
        Delete.FromTable("system_tray_menu").Row(new { id = "exit" });

        Delete.FromTable("system_icon_path").Row(new { id = "clingy-close" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy-pinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy-unpinned" });
        Delete.FromTable("system_icon_path").Row(new { id = "clingy-icon" });
    }
}
