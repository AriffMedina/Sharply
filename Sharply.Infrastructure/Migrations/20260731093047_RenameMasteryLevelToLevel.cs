using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sharply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMasteryLevelToLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MasteryLevel",
                table: "Skills",
                newName: "Level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Skills",
                newName: "MasteryLevel");
        }
    }
}
