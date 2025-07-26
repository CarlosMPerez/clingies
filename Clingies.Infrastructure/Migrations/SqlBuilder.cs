namespace Clingies.Infrastructure.Migrations;

public static class SqlBuilder
{
    public static string BuildInsertSystemTrayMenuItem(string id, string? menuType, string? parentId, string? label,
                                                        string? tooltip, bool? enabled, bool? separator,int sortOrder)
    {
        return string.Format(@"
            INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}
            WHERE NOT EXISTS (SELECT 1 FROM system_menu WHERE id = {0})
        ", SanitizeString(id), SanitizeString(menuType!), SanitizeString(parentId!),
        SanitizeString(label!), SanitizeString(tooltip!), enabled, separator, sortOrder);
    }

    public static string BuildInsertSystemTrayIcon(string id, string lightPath, string darkPath)
    {

        return string.Format(@"
            INSERT INTO system_icon_path (id, light_path, dark_path)
            SELECT {0}, {1}, {2}
            WHERE NOT EXISTS (SELECT 1 FROM system_icon_path WHERE id = {0})
        ", SanitizeString(id), SanitizeString(lightPath), SanitizeString(darkPath));
    }

    public static string BuildInsertStyle(string id, string bodyColor, string titleColor, string bodyFont,
                                        int bodyFontSize, string bodyFontColor, string? bodyFontDecorations,
                                        string titleFont, int titleFontSize, string titleFontColor, 
                                        string? titleFontDecorations, bool isDefault, bool isActive)
    {
        return string.Format(@"
            INSERT INTO styles (id, body_color, title_color, body_font, 
                                body_font_color, body_font_size, body_font_decorations, 
                                title_font, title_font_size, title_font_color, title_font_decorations, 
                                is_default, is_active)
            SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}
            WHERE NOT EXISTS (SELECT 1 FROM styles WHERE id = {0})
        ", SanitizeString(id), SanitizeString(bodyColor), SanitizeString(titleColor), SanitizeString(bodyFont),
        SanitizeString(bodyFontColor), bodyFontSize, SanitizeString(bodyFontDecorations!),
        SanitizeString(titleFont), titleFontSize, SanitizeString(titleFontColor), SanitizeString(titleFontDecorations!), 
        isDefault, isActive);
    }

    private static string SanitizeString(string val)
    {
        return string.IsNullOrEmpty(val) ? "NULL" : string.Format("'{0}'", val);
    }

}
