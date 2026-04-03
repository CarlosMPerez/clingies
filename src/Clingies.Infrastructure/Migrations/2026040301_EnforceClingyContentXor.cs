using System.Data;
using FluentMigrator;

namespace Clingies.Infrastructure.Migrations;

[Migration(2026040301)]
public class _20260403_01_EnforceClingyContentXor : Migration
{
    public override void Up()
    {
        EnsureNoExistingViolations();

        Execute.Sql(
            """
            CREATE TRIGGER IF NOT EXISTS trg_clingy_content_xor_insert
            BEFORE INSERT ON clingy_content
            FOR EACH ROW
            WHEN
                NEW.text IS NOT NULL AND length(trim(NEW.text)) > 0
                AND NEW.png IS NOT NULL AND length(NEW.png) > 0
            BEGIN
                SELECT RAISE(ABORT, 'clingy_content.text and clingy_content.png are mutually exclusive');
            END;
            """
        );

        Execute.Sql(
            """
            CREATE TRIGGER IF NOT EXISTS trg_clingy_content_xor_update
            BEFORE UPDATE ON clingy_content
            FOR EACH ROW
            WHEN
                NEW.text IS NOT NULL AND length(trim(NEW.text)) > 0
                AND NEW.png IS NOT NULL AND length(NEW.png) > 0
            BEGIN
                SELECT RAISE(ABORT, 'clingy_content.text and clingy_content.png are mutually exclusive');
            END;
            """
        );
    }

    public override void Down()
    {
        Execute.Sql("DROP TRIGGER IF EXISTS trg_clingy_content_xor_insert;");
        Execute.Sql("DROP TRIGGER IF EXISTS trg_clingy_content_xor_update;");
    }

    private void EnsureNoExistingViolations()
    {
        Execute.WithConnection((connection, transaction) =>
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = CommandType.Text;
            command.CommandText =
                """
                SELECT COUNT(*)
                FROM clingy_content
                WHERE
                    text IS NOT NULL AND length(trim(text)) > 0
                    AND png IS NOT NULL AND length(png) > 0;
                """;

            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                throw new InvalidOperationException(
                    $"Migration 2026040301 cannot be applied because {count} row(s) in clingy_content already store both text and png.");
            }
        });
    }
}
