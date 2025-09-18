using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchBugs.Persistence.Migrations;

/// <inheritdoc />
public partial class AddProfileTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "profiles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                job_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                twitter_handle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                linked_in_profile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                git_hub_profile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                time_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                preferred_language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                modified_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_profiles", x => x.id);
                table.ForeignKey(
                    name: "fk_profiles_user_user_id",
                    column: x => x.user_id,
                    principalTable: "user",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_profiles_created_on_utc",
            table: "profiles",
            column: "created_on_utc");

        migrationBuilder.CreateIndex(
            name: "ix_profiles_is_public",
            table: "profiles",
            column: "is_public");

        migrationBuilder.CreateIndex(
            name: "ix_profiles_user_id",
            table: "profiles",
            column: "user_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "profiles");
    }
}
