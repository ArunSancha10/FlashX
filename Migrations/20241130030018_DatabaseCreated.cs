using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace outofoffice.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Company_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Company_Nm = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Company_ID);
                });

            migrationBuilder.CreateTable(
                name: "UserAppConfig",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Company_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Group_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    App_Nm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    User_Mail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppConfig", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Group_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Company_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Group_Nm = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Group_ID);
                    table.ForeignKey(
                        name: "FK_Groups_Companies_Company_ID",
                        column: x => x.Company_ID,
                        principalTable: "Companies",
                        principalColumn: "Company_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageAppLists",
                columns: table => new
                {
                    MAID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Company_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Group_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    App_Nm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    App_Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    App_Channels = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Access_Token_User_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Access_Token_Txt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publish_Immd_Flag = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageAppLists", x => x.MAID);
                    table.ForeignKey(
                        name: "FK_MessageAppLists_Groups_Group_ID",
                        column: x => x.Group_ID,
                        principalTable: "Groups",
                        principalColumn: "Group_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAppMessages",
                columns: table => new
                {
                    UAID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Company_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Group_ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message_Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OOO_From_Dt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OOO_To_Dt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message_Txt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apps_To_Publish = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Publish_Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppMessages", x => x.UAID);
                    table.ForeignKey(
                        name: "FK_UserAppMessages_Groups_Group_ID",
                        column: x => x.Group_ID,
                        principalTable: "Groups",
                        principalColumn: "Group_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Company_ID",
                table: "Groups",
                column: "Company_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MessageAppLists_Group_ID",
                table: "MessageAppLists",
                column: "Group_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppMessages_Group_ID",
                table: "UserAppMessages",
                column: "Group_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageAppLists");

            migrationBuilder.DropTable(
                name: "UserAppConfig");

            migrationBuilder.DropTable(
                name: "UserAppMessages");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
