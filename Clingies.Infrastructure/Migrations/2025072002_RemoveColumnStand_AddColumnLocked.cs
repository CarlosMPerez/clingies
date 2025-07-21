using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025072002)]
public class _2025072002_RemoveColumnStand_AddColumnLocked : Migration
{
    public override void Up()
    {
        if (Schema.Table("Clingies").Column("IsStand").Exists())
        {
            Delete.Column("IsStand").FromTable("Clingies");
        }

        if (!Schema.Table("Clingies").Column("IsLocked").Exists())
        {
            // Add 'IsLocked' column
            Alter.Table("Clingies")
            .AddColumn("IsLocked")
            .AsBoolean()
            .NotNullable()
            .WithDefaultValue(false);
        }
    }

    public override void Down()
    {
        // Remove 'IsLocked' column
        if (Schema.Table("Clingies").Column("IsLocked").Exists()) Delete.Column("IsLocked").FromTable("Clingies");

        if (!Schema.Table("Clingies").Column("IsStand").Exists())
            // Re_add 'Stand' column (assuming it was originally a nullable string, adjust if needed)
            Alter.Table("Clingies")
                .AddColumn("Stand")
                .AsString()
                .Nullable();
    }

}
