using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp2OpenCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Columns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Columns");
        }
    }
}
