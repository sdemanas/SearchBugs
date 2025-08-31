using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchBugs.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    request_data = table.Column<string>(type: "text", nullable: false),
                    response_data = table.Column<string>(type: "text", nullable: true),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_on_utc",
                table: "audit_logs",
                column: "created_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_is_success",
                table: "audit_logs",
                column: "is_success");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_request_name",
                table: "audit_logs",
                column: "request_name");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_user_id",
                table: "audit_logs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");
        }
    }
}
