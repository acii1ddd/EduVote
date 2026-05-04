using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEducationUnitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserEducationUnits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEducationUnits", x => new { x.UserId, x.EducationUnitId });
                    table.ForeignKey(
                        name: "FK_UserEducationUnits_EducationUnits_EducationUnitId",
                        column: x => x.EducationUnitId,
                        principalTable: "EducationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEducationUnits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserEducationUnits_EducationUnitId",
                table: "UserEducationUnits",
                column: "EducationUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEducationUnits");
        }
    }
}
