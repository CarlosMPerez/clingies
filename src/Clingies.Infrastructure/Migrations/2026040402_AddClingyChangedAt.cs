using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2026040402)]
public class _20260404_02_AddClingyChangedAt : Migration
{
    public override void Up()
    {
        if (!Schema.Table("clingies").Exists() || Schema.Table("clingies").Column("changed_at").Exists())
            return;

        Alter.Table("clingies")
            .AddColumn("changed_at").AsDateTime().Nullable();
    }

    public override void Down()
    {
        if (Schema.Table("clingies").Exists() && Schema.Table("clingies").Column("changed_at").Exists())
            Delete.Column("changed_at").FromTable("clingies");
    }
}
