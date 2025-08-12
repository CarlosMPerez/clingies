using FluentMigrator;


namespace Clingies.Infrastructure.Migrations;
[Migration(2025062401)]
public class _2025062401_AddOnTopColumn : Migration
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
