using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace outofoffice.Migrations
{
    /// <inheritdoc />
    public partial class tabledelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HangFireJobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HangFireJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UAID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false),
                    JobId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangFireJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HangFireJobs_UserAppMessages_UAID",
                        column: x => x.UAID,
                        principalTable: "UserAppMessages",
                        principalColumn: "UAID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HangFireJobs_UAID",
                table: "HangFireJobs",
                column: "UAID");
        }
    }
}
