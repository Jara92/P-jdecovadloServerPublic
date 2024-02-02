using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTagsMultiple : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemTag_Item_ItemId",
                table: "ItemTag");

            migrationBuilder.DropIndex(
                name: "IX_ItemTag_ItemId",
                table: "ItemTag");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ItemTag");

            migrationBuilder.CreateTable(
                name: "ItemItemTag",
                columns: table => new
                {
                    ItemsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemItemTag", x => new { x.ItemsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ItemItemTag_ItemTag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "ItemTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemItemTag_Item_ItemsId",
                        column: x => x.ItemsId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemItemTag_TagsId",
                table: "ItemItemTag",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemItemTag");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "ItemTag",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemTag_ItemId",
                table: "ItemTag",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemTag_Item_ItemId",
                table: "ItemTag",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id");
        }
    }
}
