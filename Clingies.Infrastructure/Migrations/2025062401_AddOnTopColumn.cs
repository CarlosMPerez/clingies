using FluentMigrator;


namespace Clingies.Infrastructure.Migrations;
[Migration(20250624)]
public class _20250624_AddOnTopColumn : Migration
{
    public override void Up()
    {
        Alter.Table("Clingies")
            .AddColumn("IsPinned")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Column("IsPinned").FromTable("Clingies");
    }

}
