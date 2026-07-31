using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sharply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionsAndTotalXp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalXp",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XpReward = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MissionId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    XpAwarded = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionCompletions_Missions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "Missions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissionCompletions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "Description", "GroupId", "Period", "Target", "Title", "Type", "XpReward" },
                values: new object[,]
                {
                    { 1, "Registra una sesion de practica en 3 skills distintas antes de que termine el dia.", null, "Daily", 3, "Practica 3 skills hoy", "DailyPractice", 15 },
                    { 2, "Practica una skill cuya retencion haya caido por debajo de tu umbral de alerta.", null, "Daily", 1, "Rescata una skill en riesgo", "RescueRusty", 25 },
                    { 3, "Practica al menos una skill por dia durante 7 dias seguidos.", null, "Weekly", 7, "Manten tu racha 7 dias", "KeepStreak", 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionCompletions_MissionId",
                table: "MissionCompletions",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionCompletions_UserId",
                table: "MissionCompletions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionCompletions");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropColumn(
                name: "TotalXp",
                table: "Users");
        }
    }
}
