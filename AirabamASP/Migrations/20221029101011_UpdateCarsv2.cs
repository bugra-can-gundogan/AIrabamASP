using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirabamASP.Migrations
{
    public partial class UpdateCarsv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AvgFuelCons",
                table: "Cars",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Case",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "EnginePow",
                table: "Cars",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "EngineVol",
                table: "Cars",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "FuelType",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gear",
                table: "Cars",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgFuelCons",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Case",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "EnginePow",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "EngineVol",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "FuelType",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "Gear",
                table: "Cars");
        }
    }
}
