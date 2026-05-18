using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Votings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votings_CreatedById",
                table: "Votings",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings");

            migrationBuilder.DropIndex(
                name: "IX_Votings_CreatedById",
                table: "Votings");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Votings");
        }
    }
}
