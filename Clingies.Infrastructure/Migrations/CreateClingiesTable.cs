using FluentMigrator;

namespace Clingies.Infrastructure.Migrations
{
    [Migration(1)]
    public class CreateClingiesTable : Migration
    {
        public override void Up()
        {
            Create.Table("Clingies")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("Content").AsString().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable()
                .WithColumn("ModifiedAt").AsDateTime().Nullable()
                .WithColumn("IsDeleted").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("Clingies");
        }
    }
}
