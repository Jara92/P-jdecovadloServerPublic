using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class Item_AddLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Item",
                type: "TEXT",
                precision: 18,
                scale: 15,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Item",
                type: "TEXT",
                precision: 18,
                scale: 15,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Item");
        }
    }
}
