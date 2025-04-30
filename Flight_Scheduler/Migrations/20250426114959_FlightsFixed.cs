using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flight_Scheduler.Migrations
{
    /// <inheritdoc />
    public partial class FlightsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flight_Airline_AirlineId",
                table: "Flight");

            migrationBuilder.AlterColumn<int>(
                name: "AirlineId",
                table: "Flight",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flight_Airline_AirlineId",
                table: "Flight",
                column: "AirlineId",
                principalTable: "Airline",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flight_Airline_AirlineId",
                table: "Flight");

            migrationBuilder.AlterColumn<int>(
                name: "AirlineId",
                table: "Flight",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Flight_Airline_AirlineId",
                table: "Flight",
                column: "AirlineId",
                principalTable: "Airline",
                principalColumn: "Id");
        }
    }
}
