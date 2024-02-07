using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupProtocol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PickupProtocolId",
                table: "Loan",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickupProtocolId",
                table: "Image",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PickupProtocol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcceptedRefundableDeposit = table.Column<float>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupProtocol", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loan_PickupProtocolId",
                table: "Loan",
                column: "PickupProtocolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Image_PickupProtocolId",
                table: "Image",
                column: "PickupProtocolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_PickupProtocol_PickupProtocolId",
                table: "Image",
                column: "PickupProtocolId",
                principalTable: "PickupProtocol",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Loan_PickupProtocol_PickupProtocolId",
                table: "Loan",
                column: "PickupProtocolId",
                principalTable: "PickupProtocol",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_PickupProtocol_PickupProtocolId",
                table: "Image");

            migrationBuilder.DropForeignKey(
                name: "FK_Loan_PickupProtocol_PickupProtocolId",
                table: "Loan");

            migrationBuilder.DropTable(
                name: "PickupProtocol");

            migrationBuilder.DropIndex(
                name: "IX_Loan_PickupProtocolId",
                table: "Loan");

            migrationBuilder.DropIndex(
                name: "IX_Image_PickupProtocolId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "PickupProtocolId",
                table: "Loan");

            migrationBuilder.DropColumn(
                name: "PickupProtocolId",
                table: "Image");
        }
    }
}
