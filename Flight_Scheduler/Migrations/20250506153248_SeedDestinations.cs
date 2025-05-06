using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flight_Scheduler.Migrations
{
    /// <inheritdoc />
    public partial class SeedDestinations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Destinations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "John F. Kennedy (JFK), New York" },
                    { 2, "Los Angeles International (LAX), Los Angeles" },
                    { 3, "Hartsfield-Jackson Atlanta (ATL), Atlanta" },
                    { 4, "Heathrow Airport (LHR), London" },
                    { 5, "Charles de Gaulle (CDG), Paris" },
                    { 6, "Tokyo Haneda (HND), Tokyo" },
                    { 7, "Dubai International (DXB), Dubai" },
                    { 8, "Sydney Airport (SYD), Sydney" },
                    { 9, "Changi Airport (SIN), Singapore" },
                    { 10, "Hong Kong International (HKG), Hong Kong" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Destinations",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
