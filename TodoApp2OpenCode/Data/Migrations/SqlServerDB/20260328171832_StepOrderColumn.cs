using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp2OpenCode.Data.Migrations.SqlServerDB
{
    /// <inheritdoc />
    public partial class StepOrderColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Steps_ItemId",
                table: "Steps");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Steps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ItemId_Order",
                table: "Steps",
                columns: new[] { "ItemId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Steps_ItemId_Order",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Steps");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_ItemId",
                table: "Steps",
                column: "ItemId");
        }
    }
}
