using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduVote.DAL.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsFromBlockchainRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "BlockchainRecords");

            migrationBuilder.DropColumn(
                name: "SmartContractAddress",
                table: "BlockchainRecords");

            migrationBuilder.AlterColumn<string>(
                name: "BlockNumber",
                table: "BlockchainRecords",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "BlockNumber",
                table: "BlockchainRecords",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "BlockchainRecords",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmartContractAddress",
                table: "BlockchainRecords",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}
