namespace Clingies.Infrastructure.Migrations;

public static class SqlBuilder
{
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
            WHERE NOT EXISTS (SELECT 1 FROM styles WHERE style_name = {0})
        ", SanitizeString(styleName), SanitizeString(bodyColor), SanitizeString(bodyFontName),
        SanitizeString(bodyFontColor), bodyFontSize, bodyFontDecorations,
        ToSqlBoolean(isSystem), ToSqlBoolean(isDefault), ToSqlBoolean(isActive));
    }

    private static string SanitizeString(string val)
    {
        return string.IsNullOrEmpty(val) ? "NULL" : string.Format("'{0}'", val.Replace("'", "''"));
    }

    private static int ToSqlBoolean(bool value) => value ? 1 : 0;
}
