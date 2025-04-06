using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirabamASP.Migrations
{
    public partial class TestSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarsTrain");

            migrationBuilder.CreateTable(
                name: "CarsTest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<double>(type: "REAL", nullable: false),
                    Milage = table.Column<double>(type: "REAL", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    AdvertDate = table.Column<string>(type: "TEXT", nullable: false),
                    Gear = table.Column<string>(type: "TEXT", nullable: true),
                    Case = table.Column<string>(type: "TEXT", nullable: true),
                    AvgFuelCons = table.Column<double>(type: "REAL", nullable: false),
                    FuelType = table.Column<string>(type: "TEXT", nullable: true),
                    EnginePow = table.Column<double>(type: "REAL", nullable: false),
                    EngineVol = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarsTest", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarsTest");

            migrationBuilder.CreateTable(
                name: "CarsTrain",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdvertDate = table.Column<string>(type: "TEXT", nullable: false),
                    AvgFuelCons = table.Column<double>(type: "REAL", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: false),
                    Case = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    EnginePow = table.Column<double>(type: "REAL", nullable: false),
                    EngineVol = table.Column<double>(type: "REAL", nullable: false),
                    FuelType = table.Column<string>(type: "TEXT", nullable: true),
                    Gear = table.Column<string>(type: "TEXT", nullable: true),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    Milage = table.Column<double>(type: "REAL", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    Year = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarsTrain", x => x.Id);
                });
        }
    }
}
