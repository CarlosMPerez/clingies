using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025092801)]
public class _2025092801_SeedDefaultStyle : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertStyle("System", "#FFFFB8", "monospace",
                                14, "#000000", 0, true, true, true);
        Execute.Sql(sql);
        // Let's add a trigger to protect system style so it cannot be deleted 
        // by users opening the db with other tools
        Execute.Sql(@"
        CREATE TRIGGER IF NOT EXISTS trg_styles_prevent_delete_system
        BEFORE DELETE ON styles
        FOR EACH ROW
        WHEN OLD.is_system = 1
        BEGIN
        SELECT RAISE(ABORT, 'Cannot delete system style');
        END;

        CREATE TRIGGER IF NOT EXISTS trg_styles_prevent_change_system
        BEFORE UPDATE OF style_name, body_color, body_font_name, body_font_color, body_font_size, body_font_decorations, is_system, is_active ON styles
        FOR EACH ROW
        WHEN OLD.is_system = 1 
        BEGIN
        SELECT RAISE(ABORT, 'These are read-only columns for the System style');
        END;

        CREATE TRIGGER IF NOT EXISTS trg_styles_prevent_more_than_one_system
        BEFORE UPDATE OF is_system ON styles
        FOR EACH ROW
        WHEN NEW.is_system = 1 
        BEGIN
        SELECT RAISE(ABORT, 'You cannot have more than one System styles');
        END;

        -- Optional: forbid changing id of the system row
        CREATE TRIGGER IF NOT EXISTS trg_styles_prevent_id_change_system
        BEFORE UPDATE OF id ON styles
        FOR EACH ROW
        WHEN OLD.is_system = 1
        BEGIN
        SELECT RAISE(ABORT, 'Cannot change id of system style');
        END;
        ");

    }

    public override void Down()
    {
        Delete.FromTable("styles").Row(new { style_name = "System" });
    }
}
