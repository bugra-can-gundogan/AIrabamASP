using AirabamASP.Models;
using AirabamASP.Models.Entities;
using AirabamASP.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AirabamASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        AirabamDBContext _context;

        public HomeController(ILogger<HomeController> logger, AirabamDBContext con)
        {
            _context = con;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult PredictPrice()
        {
            Car car = new Car();

            var FuelTypes = _context.Cars.Select(m=> m.FuelType).Distinct().ToList();
            var Gears = _context.Cars.Select(m=>m.Gear).Distinct().ToList();
            var Brands = _context.Cars.Select(m=>m.Brand).Distinct().ToList();
            var Case = _context.Cars.Select(m=>m.Case).Distinct().ToList();
            var Colors = _context.Cars.Select(m=> m.Color).Distinct().ToList();
            var Models = _context.Cars.Select(m=>m.Model).Distinct().ToList();
            var Cities = _context.Cars.Select(m => m.City).Distinct().ToList();

            ViewBag.FuelTypes = FuelTypes;
            ViewBag.Gears = Gears;
            ViewBag.Brands = Brands;
            ViewBag.Cases = Case;
            ViewBag.Colors = Colors;
            ViewBag.Models = Models;
            ViewBag.Cities = Cities;

            return View(car);
        }

        [HttpPost]
        public IActionResult PredictPrice(Car car)
        {
            var FuelTypes = _context.Cars.Select(m => m.FuelType).Distinct().ToList();
            var Gears = _context.Cars.Select(m => m.Gear).Distinct().ToList();
            var Brands = _context.Cars.Select(m => m.Brand).Distinct().ToList();
            var Case = _context.Cars.Select(m => m.Case).Distinct().ToList();
            var Colors = _context.Cars.Select(m => m.Color).Distinct().ToList();
            var Models = _context.Cars.Select(m => m.Model).Distinct().ToList();
            var Cities = _context.Cars.Select(m => m.City).Distinct().ToList();

            ViewBag.FuelTypes = FuelTypes;
            ViewBag.Gears = Gears;
            ViewBag.Brands = Brands;
            ViewBag.Cases = Case;
            ViewBag.Colors = Colors;
            ViewBag.Models = Models;
            ViewBag.Cities = Cities;

            ModelState.Remove("Link");
            ModelState.Remove("AdvertDate");

            if (ModelState.IsValid) {
                var x = PricingModel.Predict(PricingModel.ReturnModelInput(car));
                ViewBag.Price = x.Score;
                return View(car);
            }
            else
            {
                ViewBag.Error = "Error Var";
                return View(car);
            }

            
        }

        public IActionResult Graphs() {

            var ModelsO = _context.FeatureTests.Where(x => x.Feature == "Model").ToList();
            var BrandsO = _context.FeatureTests.Where(x => x.Feature == "Brand").ToList();

            var Models = ModelsO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();

            var Brands = BrandsO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();


            Models = GetRelativeImpact(Models);
            Brands = GetRelativeImpact(Brands);

            var last10Models = Models.OrderBy(i=>i.Average).Take(10).ToList();
            var top10Models = Models.OrderBy(i => i.Average).TakeLast(10).ToList();

            var last10Brands = Brands.OrderBy(i => i.Average).Take(10).ToList();
            var top10Brands = Brands.OrderBy(i => i.Average).TakeLast(10).ToList();

            
            Dictionary<string,List<FeatureValueAverage>> ListForBrandsAndModels = new Dictionary<string, List<FeatureValueAverage>>();
            ListForBrandsAndModels.Add("Last 10 Models",last10Models);
            ListForBrandsAndModels.Add("Top 10 Models", top10Models);
            ListForBrandsAndModels.Add("Last 10 Brands", last10Brands);
            ListForBrandsAndModels.Add("Top 10 Brands", top10Brands);

            return View(ListForBrandsAndModels);
        }

        [HttpPost]
        public List<object> GetDataForGraph()
        {
            var GearsO = _context.FeatureTests.Where(x => x.Feature == "Gear").ToList();
            var CitiesO = _context.FeatureTests.Where(x => x.Feature == "City").ToList();
            var FuelTypesO = _context.FeatureTests.Where(x => x.Feature == "FuelType").ToList();

            List<object> wholeThing = new List<object>();

            List<object> data = new List<object>();

            var CasesO = _context.FeatureTests.Where(x => x.Feature == "Case").ToList();

            var Cases = CasesO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();


            Cases = GetRelativeImpact(Cases);


            List<string> labels = Cases.Select(x=>x.Value).ToList();   

            List<float> values = Cases.Select(x=>x.Average).ToList();          

            data.Add(labels);
            data.Add(values);

            wholeThing.Add(data);

            List<object> data1 = new List<object>();

            var ColorsO = _context.FeatureTests.Where(x => x.Feature == "Color").ToList();

            var Colors = ColorsO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();

            Colors = GetRelativeImpact(Colors);

            labels = Colors.Select(x => x.Value).ToList();

            values = Colors.Select(x => x.Average).ToList();

            data1.Add(labels);
            data1.Add(values);

            wholeThing.Add(data1);

            List<object> data2 = new List<object>();

            var Gears = GearsO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();


            Gears = GetRelativeImpact(Gears);

            labels = Gears.Select(x => x.Value).ToList();

            values = Gears.Select(x => x.Average).ToList();

            data2.Add(labels);
            data2.Add(values);

            wholeThing.Add(data2);

            List<object> data3 = new List<object>();

            var Cities = CitiesO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();


            Cities = GetRelativeImpact(Cities);

            Cities.OrderBy(x=> x.Average);

            Cities = Cities.Where(x => x.Value == "İstanbul" || x.Value == "Adana" || x.Value == "Ankara" || x.Value == "Mersin" || x.Value == "Şanlıurfa" || x.Value == "Kars" || x.Value == "Yozgat"|| x.Value == "Sinop"|| x.Value == "Tunceli").ToList();


            labels = Cities.Select(x => x.Value).ToList();

            values = Cities.Select(x => x.Average).ToList();

            data3.Add(labels);
            data3.Add(values);

            wholeThing.Add(data3);

            List<object> data4 = new List<object>();


            var FuelTypes = FuelTypesO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();

            FuelTypes = GetRelativeImpact(FuelTypes);

            labels = FuelTypes.Select(x => x.Value).ToList();

            values = FuelTypes.Select(x => x.Average).ToList();

            data4.Add(labels);
            data4.Add(values);

            wholeThing.Add(data4);

            List<object> data5 = new List<object>();


            var avgFuelsO = _context.FeatureTests.Where(x => x.Feature == "AvgFuelCons").ToList();

            var avgFuels = avgFuelsO.GroupBy(x => x.Value).Select(g => new FeatureValueAverage
            {
                Feature = g.First().Feature,
                Value = g.First().Value,
                Average = g.Select(p => p.Output).Average()
            }).ToList();

            avgFuels = GetRelativeImpact(avgFuels);

            labels = avgFuels.Select(x => x.Value).ToList();

            values = avgFuels.Select(x => x.Average).ToList();

            data5.Add(labels);
            data5.Add(values);

            wholeThing.Add(data5);

            return wholeThing;
        }


        private List<FeatureValueAverage> GetRelativeImpact(List<FeatureValueAverage> list)
        {

            float totalAvg = list.Select(x => x.Average).Average();

            foreach (var c in list)
            {
                float val = c.Average;

                val = val - totalAvg;

                c.Average = val;

            }

            return list;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}