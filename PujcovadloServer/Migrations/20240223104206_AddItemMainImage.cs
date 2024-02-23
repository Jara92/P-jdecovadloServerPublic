using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class AddItemMainImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainImageId",
                table: "Item",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_MainImageId",
                table: "Item",
                column: "MainImageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Image_MainImageId",
                table: "Item",
                column: "MainImageId",
                principalTable: "Image",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Image_MainImageId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_MainImageId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "MainImageId",
                table: "Item");
        }
    }
}
