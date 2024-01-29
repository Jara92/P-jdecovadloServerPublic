using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class LoanRenamePricePerUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerUnit",
                table: "Loan",
                newName: "PricePerDay");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerDay",
                table: "Loan",
                newName: "PricePerUnit");
        }
    }
}
