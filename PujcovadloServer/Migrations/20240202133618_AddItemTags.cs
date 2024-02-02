using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PujcovadloServer.Migrations
{
    /// <inheritdoc />
    public partial class AddItemTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 32, nullable: false),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemTag_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTag_ItemId",
                table: "ItemTag",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTag_Name",
                table: "ItemTag",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemTag");
        }
    }
}
