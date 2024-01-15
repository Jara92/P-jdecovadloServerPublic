using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategories2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemItemCategory_ItemCategory_ItemCategoriesId",
                table: "ItemItemCategory");

            migrationBuilder.RenameColumn(
                name: "ItemCategoriesId",
                table: "ItemItemCategory",
                newName: "CategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemItemCategory_ItemCategory_CategoriesId",
                table: "ItemItemCategory",
                column: "CategoriesId",
                principalTable: "ItemCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemItemCategory_ItemCategory_CategoriesId",
                table: "ItemItemCategory");

            migrationBuilder.RenameColumn(
                name: "CategoriesId",
                table: "ItemItemCategory",
                newName: "ItemCategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemItemCategory_ItemCategory_ItemCategoriesId",
                table: "ItemItemCategory",
                column: "ItemCategoriesId",
                principalTable: "ItemCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
