using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sharply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDecayPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DecayEmailEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "DecayRetentionThreshold",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.5);

            migrationBuilder.AddColumn<string>(
                name: "DecayStrategy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Ebbinghaus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecayEmailEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DecayRetentionThreshold",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DecayStrategy",
                table: "Users");
        }
    }
}
