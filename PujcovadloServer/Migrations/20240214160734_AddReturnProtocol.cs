using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnProtocol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReturnProtocolId",
                table: "Loan",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnProtocolId",
                table: "Image",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReturnProtocol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReturnedRefundableDeposit = table.Column<float>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnProtocol", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loan_ReturnProtocolId",
                table: "Loan",
                column: "ReturnProtocolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_ReturnProtocolId",
                table: "Image",
                column: "ReturnProtocolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_ReturnProtocol_ReturnProtocolId",
                table: "Image",
                column: "ReturnProtocolId",
                principalTable: "ReturnProtocol",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_ReturnProtocol_ReturnProtocolId",
                table: "Loan",
                column: "ReturnProtocolId",
                principalTable: "ReturnProtocol",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_ReturnProtocol_ReturnProtocolId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_Loan_ReturnProtocol_ReturnProtocolId",
                table: "Loan");

            migrationBuilder.DropTable(
                name: "ReturnProtocol");

            migrationBuilder.DropIndex(
                name: "IX_Loan_ReturnProtocolId",
                table: "Loan");

            migrationBuilder.DropIndex(
                name: "IX_Image_ReturnProtocolId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "ReturnProtocolId",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "ReturnProtocolId",
                table: "Image");
        }
    }
}
