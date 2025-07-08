using FluentMigrator;


namespace Clingies.Infrastructure.Migrations;
[Migration(20250704)]
public class _202500704_AddIsRolledAndIsStandColumns : Migration
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
