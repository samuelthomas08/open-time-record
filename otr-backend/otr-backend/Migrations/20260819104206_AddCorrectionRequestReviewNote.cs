using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionRequestReviewNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "TimeEntryCorrectionRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "TimeEntryCorrectionRequests");
        }
    }
}
