using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp2OpenCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStartEndDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Items",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Items");
        }
    }
}
