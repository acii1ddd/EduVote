using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddVotingTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VotingTargets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotingTargets_EducationUnits_EducationUnitId",
                        column: x => x.EducationUnitId,
                        principalTable: "EducationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotingTargets_Votings_VotingId",
                        column: x => x.VotingId,
                        principalTable: "Votings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VotingTargets_EducationUnitId",
                table: "VotingTargets",
                column: "EducationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingTargets_VotingId",
                table: "VotingTargets",
                column: "VotingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VotingTargets");
        }
    }
}
