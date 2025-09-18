using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchBugs.Persistence.Migrations;

/// <inheritdoc />
public partial class FixRepositoryNotificationUtcFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // For repository table: rename created_at to created_on_utc and updated_at to modified_on_utc
        migrationBuilder.RenameColumn(
            name: "created_at",
            table: "repository",
            newName: "created_on_utc");

        migrationBuilder.RenameColumn(
            name: "updated_at",
            table: "repository",
            newName: "modified_on_utc");

        // Make modified_on_utc nullable for repository
        migrationBuilder.AlterColumn<DateTime>(
            name: "modified_on_utc",
            table: "repository",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        // For notification table: rename created_at to created_on_utc and add modified_on_utc
        migrationBuilder.RenameColumn(
            name: "created_at",
            table: "notification",
            newName: "created_on_utc");

        migrationBuilder.AddColumn<DateTime>(
            name: "modified_on_utc",
            table: "notification",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "modified_on_utc",
            table: "notification");

        // Reverse the column renames for repository
        migrationBuilder.RenameColumn(
            name: "created_on_utc",
            table: "repository",
            newName: "created_at");

        migrationBuilder.RenameColumn(
            name: "modified_on_utc",
            table: "repository",
            newName: "updated_at");

        // Make updated_at non-nullable again
        migrationBuilder.AlterColumn<DateTime>(
            name: "updated_at",
            table: "repository",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime?),
            oldType: "timestamp with time zone",
            oldNullable: true);

        // Reverse the column rename for notification
        migrationBuilder.RenameColumn(
            name: "created_on_utc",
            table: "notification",
            newName: "created_at");
    }
}
