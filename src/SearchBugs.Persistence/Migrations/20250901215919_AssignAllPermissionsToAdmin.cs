using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SearchBugs.Persistence.Migrations;

/// <inheritdoc />
public partial class AssignAllPermissionsToAdmin : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 9,
            column: "name",
            value: "ViewProjectDetails");

        migrationBuilder.InsertData(
            table: "permission",
            columns: new[] { "id", "description", "name", "role_id" },
            values: new object[,]
            {
                { 38, "Can View Roles", "ViewRoles", null },
                { 39, "Can View Permissions", "ViewPermissions", null },
                { 40, "Can Assign Permission To Role", "AssignPermissionToRole", null },
                { 41, "Can Remove Permission From Role", "RemovePermissionFromRole", null },
                { 42, "Can View Role Permissions", "ViewRolePermissions", null },
                { 43, "Can Assign Role To User", "AssignRoleToUser", null },
                { 44, "Can Remove Role From User", "RemoveRoleFromUser", null }
            });

        migrationBuilder.InsertData(
            table: "role_permission",
            columns: new[] { "permission_id", "role_id" },
            values: new object[,]
            {
                { 38, 1 },
                { 39, 1 },
                { 40, 1 },
                { 41, 1 },
                { 42, 1 },
                { 43, 1 },
                { 44, 1 }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 38, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 39, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 40, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 41, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 42, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 43, 1 });

        migrationBuilder.DeleteData(
            table: "role_permission",
            keyColumns: new[] { "permission_id", "role_id" },
            keyValues: new object[] { 44, 1 });

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 38);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 39);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 40);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 41);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 42);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 43);

        migrationBuilder.DeleteData(
            table: "permission",
            keyColumn: "id",
            keyValue: 44);

        migrationBuilder.UpdateData(
            table: "permission",
            keyColumn: "id",
            keyValue: 9,
            column: "name",
            value: "ViewUserDetails");
    }
}
