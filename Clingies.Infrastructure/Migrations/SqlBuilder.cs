namespace Clingies.Infrastructure.Migrations;

public static class SqlBuilder
{
    public static string BuildInsertSystemTrayMenuItem(string id, string? menuType, string? parentId, string? label,
                                                        string? tooltip, bool? enabled, bool? separator, int sortOrder)
    {
        string _menuType = string.IsNullOrEmpty(menuType) ? "NULL" : AddApostrophes(menuType);
        string _label = string.IsNullOrEmpty(label) ? "NULL" : AddApostrophes(label);
        string _tooltip = string.IsNullOrEmpty(tooltip) ? "NULL" : AddApostrophes(tooltip);
        string _enabled = enabled.HasValue ? (enabled.Value ? "1" : "0") : "NULL";
        string _separator = separator.HasValue ? (separator.Value ? "1" : "0") : "NULL";
        string _parentId = string.IsNullOrEmpty(parentId) ? "NULL" : AddApostrophes(parentId);

        return string.Format(@"
            INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}
            WHERE NOT EXISTS (SELECT 1 FROM system_menu WHERE id = {0})
        ", AddApostrophes(id), _menuType, _parentId, _label, _tooltip, _enabled, _separator, sortOrder);
    }

    public static string BuildInsertSystemTrayIcon(string id, string lightPath, string darkPath)
    {

        return string.Format(@"
            INSERT INTO app_icon_paths (id, light_path, dark_path)
            SELECT {0}, {1}, {2}
            WHERE NOT EXISTS (SELECT 1 FROM app_icon_paths WHERE id = {0})
        ", AddApostrophes(id), AddApostrophes(lightPath), AddApostrophes(darkPath));
    }

    public static string BuildInsertClingyType(int id, string name)
    {
        return string.Format(@"
            INSERT INTO clingy_types (id, name)
            SELECT {0}, {1}
            WHERE NOT EXISTS (SELECT 1 FROM clingy_types WHERE id = {0})
        ", id, name);
    }

    private static string AddApostrophes(string val)
    {
        return string.Format("'{0}'", val);
    }

}
