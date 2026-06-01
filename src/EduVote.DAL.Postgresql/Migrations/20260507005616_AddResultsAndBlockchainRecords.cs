using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddResultsAndBlockchainRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VotingStatus",
                table: "Votings",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "VotingResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultData = table.Column<string>(type: "jsonb", nullable: false),
                    ResultHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalVotes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotingResults_Votings_VotingId",
                        column: x => x.VotingId,
                        principalTable: "Votings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VotesHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: false),
                    Network = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SmartContractAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockchainRecords_VotingResults_VotingResultId",
                        column: x => x.VotingResultId,
                        principalTable: "VotingResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainRecords_CreatedAt",
                table: "BlockchainRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainRecords_Network_Status",
                table: "BlockchainRecords",
                columns: new[] { "Network", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainRecords_TransactionHash",
                table: "BlockchainRecords",
                column: "TransactionHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainRecords_VotingId",
                table: "BlockchainRecords",
                column: "VotingId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockchainRecords_VotingResultId",
                table: "BlockchainRecords",
                column: "VotingResultId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingResults_CalculatedAt",
                table: "VotingResults",
                column: "CalculatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VotingResults_VotingId",
                table: "VotingResults",
                column: "VotingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainRecords");

            migrationBuilder.DropTable(
                name: "VotingResults");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Votings",
                newName: "VotingStatus");
        }
    }
}
