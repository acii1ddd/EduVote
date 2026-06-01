using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class addPicToCandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Candidates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Candidates");
        }
    }
}
