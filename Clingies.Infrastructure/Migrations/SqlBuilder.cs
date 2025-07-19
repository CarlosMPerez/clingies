namespace Clingies.Infrastructure.Migrations;
public static class SqlBuilder
{
    public static string BuildInsertSystemTrayItemSql(string id, string? label, string? tooltip, string? icon,
        int sortOrder, bool? enabled = true, bool? separator = false, string? parentId = null)
    {
        string _label = label ?? "NULL";
        string _tooltip = tooltip ?? "NULL";
        string _icon = icon ?? "NULL";
        string _enabled = enabled.HasValue ? (enabled.Value ? "1" : "0") : "NULL";
        string _separator = separator.HasValue ? (separator.Value ? "1" : "0") : "NULL";
        string _parentId = parentId ?? "NULL";

        return string.Format(@"
            INSERT INTO system_tray_menu (id, label, tooltip, icon, sort_order, enabled, separator, parent_id)
            SELECT '{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}
            WHERE NOT EXISTS (SELECT 1 FROM system_tray_menu WHERE id = '{0}')
        ", id, _label, _tooltip, _icon, sortOrder, _enabled, _separator, _parentId);
    }
}
