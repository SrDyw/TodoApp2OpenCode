using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TodoApp2OpenCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEntities", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TestEntities",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Este es el primer registro de prueba", "Primer Registro" },
                    { 2, "Segundo registro para verificar la conexión", "Segundo Registro" },
                    { 3, "Tercer registro - si ves esto, la conexión funciona", "Tercer Registro" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestEntities");
        }
    }
}
