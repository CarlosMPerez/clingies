using FluentMigrator;


namespace Clingies.Infrastructure.Migrations;
[Migration(2025070401)]
public class _20250070401_AddIsRolledAndIsStandColumns : Migration
{
    public override void Up()
    {
        Alter.Table("Clingies")
            .AddColumn("IsRolled")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);

        Alter.Table("Clingies")
            .AddColumn("IsStand")
                .AsBoolean()
                .NotNullable()
                .WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Column("IsRolled").FromTable("Clingies");
        Delete.Column("IsStand").FromTable("Clingies");
    }

}
