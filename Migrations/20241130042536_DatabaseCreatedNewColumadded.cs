using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace outofoffice.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseCreatedNewColumadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "MessageAppLists",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "MessageAppLists",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "MessageAppLists");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "MessageAppLists");
        }
    }
}
