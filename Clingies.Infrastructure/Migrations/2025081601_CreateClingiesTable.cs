using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025081601)]
public class _2025081601_CreateClingiesTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Clingies").Exists())
        {
            Create.Table("Clingies")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("Content").AsString().NotNullable()
                .WithColumn("PositionX").AsDouble().NotNullable().WithDefaultValue(100)
                .WithColumn("PositionY").AsDouble().NotNullable().WithDefaultValue(100)
                .WithColumn("Width").AsDouble().NotNullable().WithDefaultValue(300)
                .WithColumn("Height").AsDouble().NotNullable().WithDefaultValue(100)
                .WithColumn("IsPinned").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsRolled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsLocked").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsStanding").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("IsDeleted").AsBoolean().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable();
        }
    }

    public override void Down()
    {
        Delete.Table("Clingies");
    }
}

