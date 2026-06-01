using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class ExtendVoteModelForMultipleVotingTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_UserId_VotingId_CandidateId",
                table: "Votes");

            migrationBuilder.AlterColumn<Guid>(
                name: "CandidateId",
                table: "Votes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "RatingAnswers",
                table: "Votes",
                type: "jsonb",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedCandidateIds",
                table: "Votes",
                type: "jsonb",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextAnswer",
                table: "Votes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId_VotingId",
                table: "Votes",
                columns: new[] { "UserId", "VotingId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_UserId_VotingId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "RatingAnswers",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "SelectedCandidateIds",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "TextAnswer",
                table: "Votes");

            migrationBuilder.AlterColumn<Guid>(
                name: "CandidateId",
                table: "Votes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId_VotingId_CandidateId",
                table: "Votes",
                columns: new[] { "UserId", "VotingId", "CandidateId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
