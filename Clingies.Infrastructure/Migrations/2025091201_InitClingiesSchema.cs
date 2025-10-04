using System.Data;
using FluentMigrator;

namespace Clingies.Infrastructure.Migrations
{
    [Migration(2025091201)]
    public class _20250912_01_Init_Clingies_Schema : Migration
    {
        public override void Up()
        {
            // using Dapper/ADO: "" per-connection.

            if (!Schema.Table("clingy_types").Exists())
            {
                // --- Lookup: clingy_types (manual IDs, protected from delete if in use)
                Create.Table("clingy_types")
                    .WithColumn("id").AsInt32().PrimaryKey() // manual IDs
                    .WithColumn("name").AsString().NotNullable();

                Create.Index("ux_clingy_types_name")
                    .OnTable("clingy_types")
                    .WithOptions().Unique()
                    .OnColumn("name").Ascending();
            }

            if (!Schema.Table("styles").Exists())
            {
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

            if (!Schema.Table("clingies").Exists())
            {
                // --- Root: clingies (auto rowid)
                Create.Table("clingies")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("type_id").AsInt32().NotNullable()
                    .ForeignKey("fk_clingies_type", "clingy_types", "id")
                    .OnDelete(Rule.None).OnUpdate(Rule.None)
                .WithColumn("title").AsString().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

                Create.Index("ix_clingies_type_id")
                    .OnTable("clingies")
                    .OnColumn("type_id").Ascending();
            }

            if (!Schema.Table("clingy_properties").Exists())
            {
                // --- 1:1 child: clingy_properties (shared PK, cascade on clingy delete)
                Create.Table("clingy_properties")
                .WithColumn("id").AsInt32().PrimaryKey() // shared-PK; no Identity
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

            if (!Schema.Table("clingy_content").Exists())
            {
                // --- 1:1 child: clingy_content (shared PK, cascade on clingy delete)
                Create.Table("clingy_content")
                .WithColumn("id").AsInt32().PrimaryKey() // shared-PK; no Identity
                    .ForeignKey("fk_content_clingy", "clingies", "id")
                    .OnDelete(Rule.Cascade).OnUpdate(Rule.Cascade)
                .WithColumn("text").AsString(int.MaxValue).Nullable() // app ensures XOR with png
                .WithColumn("png").AsBinary().Nullable();
            }

            if (!Schema.Table("app_icon_paths").Exists())
            {
                // --- Non-menu icons (app-wide)
                Create.Table("app_icon_paths")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("light_path").AsString().Nullable()
                .WithColumn("dark_path").AsString().Nullable();
            }

            if (!Schema.Table("system_menu").Exists())
            {
                // --- System menu (self-referencing tree; cascade delete subtrees)
                Create.Table("system_menu")
                .WithColumn("id").AsString().PrimaryKey()
                .WithColumn("menu_type").AsString().NotNullable()
                .WithColumn("parent_id").AsInt32().Nullable()
                    .ForeignKey("fk_system_menu_parent", "system_menu", "id")
                    .OnDelete(Rule.Cascade).OnUpdate(Rule.Cascade)
                .WithColumn("label").AsString().Nullable()
                .WithColumn("tooltip").AsString().Nullable()
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("separator").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0);

                Create.Index("ux_system_menu_parent_sort")
                    .OnTable("system_menu")
                    .WithOptions().Unique()
                    .OnColumn("parent_id").Ascending()
                    .OnColumn("sort_order").Ascending();
            }
        }

        public override void Down()
        {
            // Drop in reverse dependency order

            Delete.Index("ux_system_menu_parent_sort").OnTable("system_menu");
            if (Schema.Table("system_menu").Exists()) Delete.Table("system_menu");

            if (Schema.Table("app_icon_paths").Exists()) Delete.Table("app_icon_paths");

            if (Schema.Table("clingy_content").Exists()) Delete.Table("clingy_content");

            if (Schema.Table("clingy_properties").Exists()) Delete.Table("clingy_properties");

            Delete.Index("ix_clingies_type_id").OnTable("clingies");
            if (Schema.Table("clingies").Exists()) Delete.Table("clingies");

            Delete.Index("ux_clingy_types_name").OnTable("clingy_types");
            if (Schema.Table("clingy_types").Exists()) Delete.Table("clingy_types");
        }
    }
}
