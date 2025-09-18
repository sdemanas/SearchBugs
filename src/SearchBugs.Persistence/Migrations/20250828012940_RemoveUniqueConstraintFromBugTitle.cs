using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchBugs.Persistence.Migrations;

/// <inheritdoc />
public partial class RemoveUniqueConstraintFromBugTitle : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_bug_project_id_title",
            table: "bug");

        migrationBuilder.CreateIndex(
            name: "ix_bug_project_id",
            table: "bug",
            column: "project_id");

        migrationBuilder.CreateIndex(
            name: "ix_bug_title",
            table: "bug",
            column: "title");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_bug_project_id",
            table: "bug");

        migrationBuilder.DropIndex(
            name: "ix_bug_title",
            table: "bug");

        migrationBuilder.CreateIndex(
            name: "ix_bug_project_id_title",
            table: "bug",
            columns: new[] { "project_id", "title" },
            unique: true);
    }
}
