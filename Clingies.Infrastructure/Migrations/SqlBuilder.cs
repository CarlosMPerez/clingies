namespace Clingies.Infrastructure.Migrations;

public static class SqlBuilder
{
    public static string BuildInsertSystemTrayMenuItem(string id, string? menuType, string? parentId,
                                                        string? label, string? tooltip, bool? enabled,
                                                        bool? separator, int sortOrder)
    {
        string _menuType = string.IsNullOrEmpty(menuType) ? "NULL" : SanitizeString(menuType);
        string _label = string.IsNullOrEmpty(label) ? "NULL" : SanitizeString(label);
        string _tooltip = string.IsNullOrEmpty(tooltip) ? "NULL" : SanitizeString(tooltip);
        string _enabled = enabled.HasValue ? (enabled.Value ? "1" : "0") : "NULL";
        string _separator = separator.HasValue ? (separator.Value ? "1" : "0") : "NULL";
        string _parentId = string.IsNullOrEmpty(parentId) ? "NULL" : SanitizeString(parentId);

        return string.Format(@"
            INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}
            WHERE NOT EXISTS (SELECT 1 FROM system_menu WHERE id = {0})
        ", SanitizeString(id), _menuType, _parentId, _label, _tooltip, _enabled, _separator, sortOrder);
    }

    public static string BuildInsertSystemTrayIcon(string id, string lightPath, string darkPath)
    {

        return string.Format(@"
            INSERT INTO app_icon_paths (id, light_path, dark_path)
            SELECT {0}, {1}, {2}
            WHERE NOT EXISTS (SELECT 1 FROM app_icon_paths WHERE id = {0})
        ", SanitizeString(id), SanitizeString(lightPath), SanitizeString(darkPath));
    }

    public static string BuildInsertClingyType(int id, string name)
    {
        return string.Format(@"
            INSERT INTO clingy_types (id, name)
            SELECT {0}, {1}
            WHERE NOT EXISTS (SELECT 1 FROM clingy_types WHERE id = {0})
        ", id, SanitizeString(name));
    }

    public static string BuildInsertStyle(string styleName, string bodyColor, string bodyFontName,
                                        int bodyFontSize, string bodyFontColor, int bodyFontDecorations,
                                        bool isSystem, bool isDefault, bool isActive)
    {
        return string.Format(@"
            INSERT INTO styles (style_name, body_color, body_font_name, 
                                body_font_color, body_font_size, body_font_decorations, 
                                is_system, is_default, is_active)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}
            WHERE NOT EXISTS (SELECT 1 FROM styles WHERE id = {0})
        ", SanitizeString(styleName), SanitizeString(bodyColor), SanitizeString(bodyFontName),
        SanitizeString(bodyFontColor), bodyFontSize, bodyFontDecorations,
        isSystem, isDefault, isActive);
    }

    private static string SanitizeString(string val)
    {
        return string.IsNullOrEmpty(val) ? "NULL" : string.Format("'{0}'", val);
    }

}
