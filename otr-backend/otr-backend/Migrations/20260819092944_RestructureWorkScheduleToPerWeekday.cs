using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace otr_backend.Migrations
{
    /// <inheritdoc />
    public partial class RestructureWorkScheduleToPerWeekday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeklyHours",
                table: "WorkSchedules",
                newName: "WednesdayHours");

            migrationBuilder.AddColumn<decimal>(
                name: "FridayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MondayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaturdayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SundayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThursdayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TuesdayHours",
                table: "WorkSchedules",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FridayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "MondayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SundayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "ThursdayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "TuesdayHours",
                table: "WorkSchedules");

            migrationBuilder.RenameColumn(
                name: "WednesdayHours",
                table: "WorkSchedules",
                newName: "WeeklyHours");
        }
    }
}
