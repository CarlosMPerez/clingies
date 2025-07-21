using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2025072101)]
public class _2025072101_AddClinncyLockedImageToGeneralIcons : Migration
{
    public override void Up()
    {
        string sql = SqlBuilder.BuildInsertSystemTrayIcon("clingy_locked",
            "avares://Clingies/Assets/clingy_locked.png",
            "avares://Clingies/Assets/clingy_locked.png");
        Execute.Sql(sql);
    }

    public override void Down()
    {
        Delete.FromTable("system_icon_path").Row(new { id = "clingy_locked" });
    }
}
