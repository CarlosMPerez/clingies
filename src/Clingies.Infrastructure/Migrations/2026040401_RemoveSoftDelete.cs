using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2026040401)]
public class _20260404_01_RemoveSoftDelete : Migration
{
    public override void Up()
    {
        if (Schema.Table("clingies").Exists() && Schema.Table("clingies").Column("is_deleted").Exists())
            Delete.Column("is_deleted").FromTable("clingies");
    }

    public override void Down()
    {
        if (!Schema.Table("clingies").Exists() || Schema.Table("clingies").Column("is_deleted").Exists())
            return;

        Alter.Table("clingies")
            .AddColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Execute.Sql(
            """
            UPDATE clingies
            SET is_deleted = 1
            WHERE type_id = 4
            """);
    }
}
