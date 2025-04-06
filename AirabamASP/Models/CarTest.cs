using System.ComponentModel.DataAnnotations;

namespace AirabamASP.Models
{
    public class CarTest
    {
        [Key]
        public int Id { get; set; }
        public string Link { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string City { get; set; }
        public string Color { get; set; }
        public double Year { get; set; }
        public double Milage { get; set; }
        public double Price { get; set; }
        public string AdvertDate { get; set; }
        public string? Gear { get; set; }
        public string? Case { get; set; }
        public double AvgFuelCons { get; set; }
        public string? FuelType { get; set; }
        public double EnginePow { get; set; }
        public double EngineVol { get; set; }
    }
}
