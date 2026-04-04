using System.Data;
using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2026031801)]
public class _20260318_01_RebuildSchema : Migration
{
    public override void Up()
    {
        EnsureClingyTypesTable();
        EnsureStylesTable();
        EnsureClingiesTable();
        EnsureClingyPropertiesTable();
        EnsureClingyContentTable();

        SeedClingyTypes();
        SeedDefaultStyle();
        EnsureStyleProtectionTriggers();
        DropObsoleteMenuTables();
    }

    public override void Down()
    {
        DropObsoleteMenuTables();

        if (Schema.Table("clingy_content").Exists())
            Delete.Table("clingy_content");

        if (Schema.Table("clingy_properties").Exists())
            Delete.Table("clingy_properties");

        if (Schema.Table("clingies").Exists())
            Delete.Table("clingies");

        if (Schema.Table("styles").Exists())
            Delete.Table("styles");

        if (Schema.Table("clingy_types").Exists())
            Delete.Table("clingy_types");
    }

    private void EnsureClingyTypesTable()
    {
        if (Schema.Table("clingy_types").Exists())
            return;

        Create.Table("clingy_types")
            .WithColumn("id").AsInt32().PrimaryKey()
            .WithColumn("name").AsString().NotNullable();

        Create.Index("ux_clingy_types_name")
            .OnTable("clingy_types")
            .WithOptions().Unique()
            .OnColumn("name").Ascending();
    }

    private void EnsureStylesTable()
    {
        if (Schema.Table("styles").Exists())
            return;

        Create.Table("styles")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("style_name").AsString().NotNullable()
            .WithColumn("body_color").AsString().NotNullable()
            .WithColumn("body_font_name").AsString().NotNullable()
            .WithColumn("body_font_color").AsString().NotNullable()
            .WithColumn("body_font_size").AsInt32().NotNullable()
            .WithColumn("body_font_decorations").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_system").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_default").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true);
    }

    private void EnsureClingiesTable()
    {
        if (Schema.Table("clingies").Exists())
            return;

        Create.Table("clingies")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("type_id").AsInt32().NotNullable()
                .ForeignKey("fk_clingies_type", "clingy_types", "id")
                .OnDelete(Rule.None).OnUpdate(Rule.None)
            .WithColumn("title").AsString().NotNullable()
            .WithColumn("created_at").AsDateTime().NotNullable()
            .WithColumn("changed_at").AsDateTime().Nullable();

        Create.Index("ix_clingies_type_id")
            .OnTable("clingies")
            .OnColumn("type_id").Ascending();
    }

    private void EnsureClingyPropertiesTable()
    {
        if (Schema.Table("clingy_properties").Exists())
            return;

        Create.Table("clingy_properties")
            .WithColumn("id").AsInt32().PrimaryKey()
                .ForeignKey("fk_props_clingy", "clingies", "id")
                .OnDelete(Rule.Cascade).OnUpdate(Rule.Cascade)
            .WithColumn("position_x").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("position_y").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("width").AsInt32().NotNullable().WithDefaultValue(240)
            .WithColumn("height").AsInt32().NotNullable().WithDefaultValue(160)
            .WithColumn("is_pinned").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_rolled").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_locked").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("is_standing").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("style_id").AsInt32().NotNullable().WithDefaultValue(1)
                .ForeignKey("fk_clingy_style", "styles", "id")
                .OnDelete(Rule.None).OnUpdate(Rule.None);
    }

    private void EnsureClingyContentTable()
    {
        if (Schema.Table("clingy_content").Exists())
            return;

        Create.Table("clingy_content")
            .WithColumn("id").AsInt32().PrimaryKey()
                .ForeignKey("fk_content_clingy", "clingies", "id")
                .OnDelete(Rule.Cascade).OnUpdate(Rule.Cascade)
            .WithColumn("text").AsString(int.MaxValue).Nullable()
            .WithColumn("png").AsBinary().Nullable();
    }

    private void SeedClingyTypes()
    {
        Execute.Sql(SqlBuilder.BuildInsertClingyType(1, "desktop"));
        Execute.Sql(SqlBuilder.BuildInsertClingyType(2, "sleeping"));
        Execute.Sql(SqlBuilder.BuildInsertClingyType(3, "recurring"));
        Execute.Sql(SqlBuilder.BuildInsertClingyType(4, "closed"));
        Execute.Sql(SqlBuilder.BuildInsertClingyType(5, "stored"));
    }

    private void SeedDefaultStyle()
    {
        Execute.Sql(SqlBuilder.BuildInsertStyle(
            AppConstants.SystemStyle.Name,
            AppConstants.SystemStyle.BodyColor,
            AppConstants.SystemStyle.BodyFontName,
            AppConstants.SystemStyle.BodyFontSize,
            AppConstants.SystemStyle.BodyFontColor,
            AppConstants.SystemStyle.BodyFontDecorations,
            true,
            true,
            true));
    }

    private void EnsureStyleProtectionTriggers()
    {
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

        CREATE TRIGGER IF NOT EXISTS trg_styles_prevent_id_change_system
        BEFORE UPDATE OF id ON styles
        FOR EACH ROW
        WHEN OLD.is_system = 1
        BEGIN
            SELECT RAISE(ABORT, 'Cannot change id of system style');
        END;
        ");
    }

    private void DropObsoleteMenuTables()
    {
        if (Schema.Table("system_menu").Exists())
            Delete.Table("system_menu");

        if (Schema.Table("app_icon_paths").Exists())
            Delete.Table("app_icon_paths");
    }
}
