using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025071802)]
public class _2025071802_InitialSeedMenuData : Migration
{
    public override void Up()
    {
        // Avoid duplicate inserts by checking for each row's existence
        InsertItem("new", "New", "Create a new Clingy", "avares://Clingies/Assets/menu-new.png", 1);
        InsertItem("settings", "Settings", "Access the Settings window", "avares://Clingies/Assets/menu-settings.png", 2);
        InsertItem("sep1", null, null, null, 3, true, true, null);
        InsertItem("exit", "Exit", "Close the application", "avares://Clingies/Assets/menu-close.png", 4);
    }

    public override void Down()
    {
        Delete.FromTable("system_tray_menu").Row(new { id = "new" });
        Delete.FromTable("system_tray_menu").Row(new { id = "settings" });
        Delete.FromTable("system_tray_menu").Row(new { id = "sep1" });
        Delete.FromTable("system_tray_menu").Row(new { id = "exit" });
    }

    private void InsertItem(string id, string? label, string? tooltip, string? icon,
            int sort_order, bool? enabled = true, bool? separator = false,
            string? parent_id = null)
    {
        string _label = label == null ? "NULL" : label;
        string _tooltip = tooltip == null ? "NULL" : tooltip;
        string _icon = icon == null ? "NULL" : icon;
        string _enabled = enabled.HasValue ? ((bool)enabled! ? "1" : "0") : "NULL";
        string _separator = separator.HasValue ? ((bool)separator! ? "1" : "0") : "NULL";
        string _parent_id = parent_id == null ? "NULL" : parent_id;

        Execute.Sql(string.Format(@"
            INSERT INTO system_tray_menu (id, label, tooltip, icon, sort_order, enabled, separator, parent_id)
            SELECT '{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}
            WHERE NOT EXISTS (SELECT 1 FROM system_tray_menu WHERE id = '{0}')
        ", id, _label, _tooltip, _icon, sort_order, _enabled, _separator, _parent_id));
    }
}
