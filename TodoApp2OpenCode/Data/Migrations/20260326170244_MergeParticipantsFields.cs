using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp2OpenCode.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeParticipantsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantIds",
                table: "Boards");

            migrationBuilder.RenameColumn(
                name: "ParticipantNamesJson",
                table: "Boards",
                newName: "ParticipantsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParticipantsJson",
                table: "Boards",
                newName: "ParticipantNamesJson");

            migrationBuilder.AddColumn<string>(
                name: "ParticipantIds",
                table: "Boards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
