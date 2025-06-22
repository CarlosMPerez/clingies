using System;
using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025062201)]
public class AddClingyPositionColumns : Migration
{
    public override void Up()
    {
        Alter.Table("Clingies")
            .AddColumn("PositionX").AsDouble().NotNullable();

        Alter.Table("Clingies")
            .AddColumn("PositionY").AsDouble().NotNullable();

        Alter.Table("Clingies")
            .AddColumn("Width").AsDouble().NotNullable();

        Alter.Table("Clingies")
            .AddColumn("Height").AsDouble().NotNullable();
    }

    public override void Down()
    {
        Delete.Column("PositionX").FromTable("Clingies");
        Delete.Column("PositionY").FromTable("Clingies");
        Delete.Column("Width").FromTable("Clingies");
        Delete.Column("Height").FromTable("Clingies");        
    }
}
