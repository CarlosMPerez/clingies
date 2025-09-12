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

            // --- Lookup: clingy_types (manual IDs, protected from delete if in use)
            Create.Table("clingy_types")
                .WithColumn("id").AsInt32().PrimaryKey() // manual IDs
                .WithColumn("name").AsString().NotNullable();

            Create.Index("ux_clingy_types_name")
                .OnTable("clingy_types")
                .WithOptions().Unique()
                .OnColumn("name").Ascending();

            // --- Root: clingies (auto rowid)
            Create.Table("clingies")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("type_id").AsInt32().NotNullable()
                .WithColumn("title").AsString().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.Index("ix_clingies_type_id")
                .OnTable("clingies")
                .OnColumn("type_id").Ascending();

            // FK: clingies.type_id -> clingy_types.id (RESTRICT/NONE on delete)
            Create.ForeignKey("fk_clingies_type")
                .FromTable("clingies").ForeignColumn("type_id")
                .ToTable("clingy_types").PrimaryColumn("id")
                .OnDelete(Rule.None).OnUpdate(Rule.None);

            // --- 1:1 child: clingy_properties (shared PK, cascade on clingy delete)
            Create.Table("clingy_properties")
                .WithColumn("id").AsInt32().PrimaryKey() // shared-PK; no Identity
                .WithColumn("position_x").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("position_y").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("width").AsInt32().NotNullable().WithDefaultValue(240)
                .WithColumn("height").AsInt32().NotNullable().WithDefaultValue(160)
                .WithColumn("is_pinned").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_rolled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_locked").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("is_standing").AsBoolean().NotNullable().WithDefaultValue(false);

            Create.ForeignKey("fk_props_clingy")
                .FromTable("clingy_properties").ForeignColumn("id")
                .ToTable("clingies").PrimaryColumn("id")
                .OnDeleteOrUpdate(Rule.Cascade);

            // --- 1:1 child: clingy_content (shared PK, cascade on clingy delete)
            Create.Table("clingy_content")
                .WithColumn("id").AsInt32().PrimaryKey() // shared-PK; no Identity
                .WithColumn("text").AsString(int.MaxValue).Nullable() // app ensures XOR with png
                .WithColumn("png").AsBinary().Nullable();

            Create.ForeignKey("fk_content_clingy")
                .FromTable("clingy_content").ForeignColumn("id")
                .ToTable("clingies").PrimaryColumn("id")
                .OnDeleteOrUpdate(Rule.Cascade);

            // --- Non-menu icons (app-wide)
            Create.Table("app_icon_paths")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("light_path").AsString().Nullable()
                .WithColumn("dark_path").AsString().Nullable();

            // --- System menu (self-referencing tree; cascade delete subtrees)
            Create.Table("system_menu")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("menu_type").AsString().NotNullable()
                .WithColumn("parent_id").AsInt32().Nullable()
                .WithColumn("label").AsString().NotNullable()
                .WithColumn("tooltip").AsString().Nullable()
                .WithColumn("enabled").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("separator").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0);

            Create.ForeignKey("fk_system_menu_parent")
                .FromTable("system_menu").ForeignColumn("parent_id")
                .ToTable("system_menu").PrimaryColumn("id")
                .OnDeleteOrUpdate(Rule.Cascade);

            Create.Index("ux_system_menu_parent_sort")
                .OnTable("system_menu")
                .WithOptions().Unique()
                .OnColumn("parent_id").Ascending()
                .OnColumn("sort_order").Ascending();
        }

        public override void Down()
        {
            // Drop in reverse dependency order

            Delete.Index("ux_system_menu_parent_sort").OnTable("system_menu");
            Delete.ForeignKey("fk_system_menu_parent").OnTable("system_menu");
            Delete.Table("system_menu");

            Delete.Table("app_icon_paths");

            Delete.ForeignKey("fk_content_clingy").OnTable("clingy_content");
            Delete.Table("clingy_content");

            Delete.ForeignKey("fk_props_clingy").OnTable("clingy_properties");
            Delete.Table("clingy_properties");

            Delete.ForeignKey("fk_clingies_type").OnTable("clingies");
            Delete.Index("ix_clingies_type_id").OnTable("clingies");
            Delete.Table("clingies");

            Delete.Index("ux_clingy_types_name").OnTable("clingy_types");
            Delete.Table("clingy_types");
        }
    }
}
