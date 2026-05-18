using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class SetIsRequiredForCreatedById : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "Votings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "Votings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Votings_Users_CreatedById",
                table: "Votings",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
