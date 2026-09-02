using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otr_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLeftoverSeedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The "Added Data" migration seeded a dev-only placeholder row directly into Users
            // (id 1), which meant a fresh database was never actually empty — so the "first
            // person to register becomes Superadmin" logic in AuthController could never fire.
            // Only removes it if it still looks untouched (never actually registered/logged in
            // through the app), so a real account that happens to reuse id 1 is left alone.
            migrationBuilder.Sql(
                """
                DELETE FROM "Users"
                WHERE "Id" = 1
                  AND "Email" = 'me@samuel-thomas.de'
                  AND ("HashedPassword" IS NULL OR "HashedPassword" = '')
                  AND NOT EXISTS (SELECT 1 FROM "UserRoleMappings" WHERE "UserId" = 1)
                  AND NOT EXISTS (SELECT 1 FROM "UserTeamMappings" WHERE "UserId" = 1)
                  AND NOT EXISTS (SELECT 1 FROM "TimeEntries" WHERE "UserId" = 1)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deleted data isn't restored on rollback.
        }
    }
}
