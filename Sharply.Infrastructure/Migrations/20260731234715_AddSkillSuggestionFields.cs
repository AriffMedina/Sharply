using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sharply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillSuggestionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentSuggestion",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuggestionGeneratedAt",
                table: "Skills",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentSuggestion",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SuggestionGeneratedAt",
                table: "Skills");
        }
    }
}
