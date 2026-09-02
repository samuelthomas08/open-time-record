using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace otr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeEntryBreaksAndBreakSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BreakReasonRequired",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TimeEntryBreaks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeEntryId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntryBreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEntryBreaks_TimeEntries_TimeEntryId",
                        column: x => x.TimeEntryId,
                        principalTable: "TimeEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntryBreaks_TimeEntryId",
                table: "TimeEntryBreaks",
                column: "TimeEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeEntryBreaks");

            migrationBuilder.DropColumn(
                name: "BreakReasonRequired",
                table: "AppSettings");
        }
    }
}
