namespace Clingies.Infrastructure.Migrations;

public static class SqlBuilder
{
    public static string BuildInsertSystemTrayMenuItem(string id, string? label, string? tooltip, int sortOrder, bool? enabled = true, bool? separator = false, string? parentId = null)
    {
        string _label = string.IsNullOrEmpty(label) ? "NULL" : AddApostrophes(label);
        string _tooltip = string.IsNullOrEmpty(tooltip) ? "NULL" : AddApostrophes(tooltip);
        string _enabled = enabled.HasValue ? (enabled.Value ? "1" : "0") : "NULL";
        string _separator = separator.HasValue ? (separator.Value ? "1" : "0") : "NULL";
        string _parentId = string.IsNullOrEmpty(parentId) ? "NULL" : AddApostrophes(parentId);

        return string.Format(@"
            INSERT INTO system_tray_menu (id, label, tooltip, enabled, separator, parent_id, sort_order)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}
            WHERE NOT EXISTS (SELECT 1 FROM system_tray_menu WHERE id = {0})
        ", AddApostrophes(id), _label, _tooltip, _enabled, _separator, _parentId, sortOrder);
    }

    public static string BuildInsertSystemTrayIcon(string id, string lightPath, string darkPath)
    {

        return string.Format(@"
            INSERT INTO system_icon_path (id, light_path, dark_path)
            SELECT {0}, {1}, {2}
            WHERE NOT EXISTS (SELECT 1 FROM system_icon_path WHERE id = {0})
        ", AddApostrophes(id), AddApostrophes(lightPath), AddApostrophes(darkPath));
    }

    private static string AddApostrophes(string val)
    {
        return string.Format("'{0}'", val);
    }

}
