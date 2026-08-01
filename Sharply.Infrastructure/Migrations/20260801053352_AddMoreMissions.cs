using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sharply.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "Description", "GroupId", "Period", "Target", "Title", "Type", "XpReward" },
                values: new object[,]
                {
                    { 4, "Registra una sesion de practica en cualquier skill antes de que termine el dia.", null, "Daily", 1, "Practica al menos 1 skill hoy", "DailyPractice", 5 },
                    { 5, "Registra una sesion de practica en 5 skills distintas en el mismo dia.", null, "Daily", 5, "Practica 5 skills hoy", "DailyPractice", 30 },
                    { 6, "Practica al menos una skill por dia durante 3 dias seguidos.", null, "Weekly", 3, "Manten tu racha 3 dias", "KeepStreak", 20 },
                    { 7, "Practica al menos una skill por dia durante 14 dias seguidos.", null, "Weekly", 14, "Manten tu racha 14 dias", "KeepStreak", 100 },
                    { 8, "Practica una skill cuya retencion haya caido por debajo de tu umbral de alerta, al menos una vez esta semana.", null, "Weekly", 1, "Rescata una skill en riesgo esta semana", "RescueRusty", 40 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
