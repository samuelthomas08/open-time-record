using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitationIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "UserInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "UserInvitations");
        }
    }
}
