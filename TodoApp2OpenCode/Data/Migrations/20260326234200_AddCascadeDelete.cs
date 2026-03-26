using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp2OpenCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Boards_TodoBoardId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Boards_TodoBoardId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Items_TodoItemId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_TodoItemId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Columns_TodoBoardId",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "TodoItemId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "TodoBoardId",
                table: "Columns");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ItemId",
                table: "Steps",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_BoardId",
                table: "Columns",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Boards_TodoBoardId",
                table: "Items",
                column: "TodoBoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Items_ItemId",
                table: "Steps",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Columns_Boards_BoardId",
                table: "Columns");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Boards_TodoBoardId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Items_ItemId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_ItemId",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Columns_BoardId",
                table: "Columns");

            migrationBuilder.AddColumn<string>(
                name: "TodoItemId",
                table: "Steps",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TodoBoardId",
                table: "Columns",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_TodoItemId",
                table: "Steps",
                column: "TodoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_TodoBoardId",
                table: "Columns",
                column: "TodoBoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Boards_TodoBoardId",
                table: "Columns",
                column: "TodoBoardId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Boards_TodoBoardId",
                table: "Items",
                column: "TodoBoardId",
                principalTable: "Boards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Items_TodoItemId",
                table: "Steps",
                column: "TodoItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }
    }
}
