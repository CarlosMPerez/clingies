using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025062001)]
public class _2025062001_CreateClingiesTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("clingies").Exists())
        {
            Create.Table("clingies")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("title").AsString().NotNullable()
                .WithColumn("content").AsString().NotNullable()
                .WithColumn("position_x").AsDouble().NotNullable()
                .WithColumn("position_y").AsDouble().NotNullable()
                .WithColumn("width").AsDouble().NotNullable()
                .WithColumn("height").AsDouble().NotNullable()
                .WithColumn("style").AsString().NotNullable().WithDefaultValue("default")
                .WithColumn("is_pinned").AsBoolean().NotNullable()
                .WithColumn("is_locked").AsBoolean().NotNullable()
                .WithColumn("is_rolled").AsBoolean().NotNullable()
                .WithColumn("is_deleted").AsBoolean().NotNullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("modified_at").AsDateTime().Nullable();
        }
    }

    public override void Down()
    {
        Delete.Table("clingies");
    }
}

