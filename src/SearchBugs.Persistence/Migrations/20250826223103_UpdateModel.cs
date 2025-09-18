using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchBugs.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateModel : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "role_id",
            table: "permission",
            type: "integer",
            nullable: true);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 1,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 2,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 3,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 4,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 5,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 6,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 7,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 8,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 9,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 10,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 11,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 12,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 13,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 14,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 15,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 16,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 17,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 18,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 19,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 20,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 21,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 22,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 23,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 24,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 25,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 26,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 28,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 29,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 30,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 31,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 32,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 33,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 34,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 35,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 36,
            column: "role_id",
            value: null);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 37,
            column: "role_id",
            value: null);

        migrationBuilder.CreateIndex(
            name: "ix_permission_role_id",
            table: "permission",
            column: "role_id");

        migrationBuilder.AddForeignKey(
            name: "fk_permission_roles_role_id",
            table: "permission",
            column: "role_id",
            principalTable: "role",
            principalColumn: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_permission_roles_role_id",
            table: "permission");

        migrationBuilder.DropIndex(
            name: "ix_permission_role_id",
            table: "permission");

        migrationBuilder.DropColumn(
            name: "role_id",
            table: "permission");
    }
}
