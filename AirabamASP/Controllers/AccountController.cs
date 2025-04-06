using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AirabamASP.Models;
using AirabamASP.Models.Entities;
using AirabamASP.Providers;
using AirabamASP.Providers.Repositories;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static AirabamASP.Providers.PricingModel;

namespace AirabamASP.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly IUserRepository _userRepository;
        AirabamDBContext _context;
        public AccountController(IUserManager userManager, IUserRepository userRepository, AirabamDBContext con)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _context = con;
        }

        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View(this.User.Claims.ToDictionary(x => x.Type, x => x.Value));
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _userRepository.Validate(model);

            if (user == null) return View(model);

            await _userManager.SignIn(this.HttpContext, user, false);

            return LocalRedirect("~/Home/Index");
        }

        public IActionResult Register()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterVm model)
        {
            if (true)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = _userRepository.Register(model);

            await _userManager.SignIn(this.HttpContext, user, false);

            return LocalRedirect("~/Home/Index");
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await _userManager.SignOut(this.HttpContext);
            return RedirectPermanent("~/Home/Index");
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }

        [Authorize]
        public IActionResult ScrapeTest()
        {
            for (int i = 0; i <= 14; i++)
            {
                var response = CallUrl($"https://www.arabam.com/ikinci-el/otomobil?days={i}&sort=startedAt.asc").Result;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                int counter = 0;
                int existedCounter = 0;
                string nextPage = "";

                var rows = doc.DocumentNode.SelectNodes("//tr[@class='listing-list-item pr should-hover bg-white']");
                while (counter > -999)
                {
                    if (counter != 0)
                    {
                        nextPage = $"https://www.arabam.com/ikinci-el/otomobil?days={i}&sort=startedAt.asc&;take=50&page={counter}";
                        var res = CallUrl(nextPage);
                        if (!res.IsFaulted)
                        {
                            response = res.Result;
                        }
                        else
                        {
                            Console.WriteLine("Bitti");
                            break;
                        }
                        HtmlDocument newDoc = new HtmlDocument();
                        newDoc.LoadHtml(response);
                        rows = newDoc.DocumentNode.SelectNodes("//tr[@class='listing-list-item pr should-hover bg-white']");
                    }

                    foreach (var item in rows)
                    {
                        var model = item.SelectSingleNode("./td[2]/h3/div").InnerHtml;
                        var brand = model.ToString().Split(" ")[0];
                        var year = item.SelectSingleNode("./td[4]/div[1]/a").InnerHtml;
                        var link = "https://www.arabam.com/" + item.SelectSingleNode("./td[4]/div[1]/a").Attributes[0].Value;
                        var km = item.SelectSingleNode("./td[5]/div[1]/a").InnerHtml;
                        var price = item.SelectSingleNode("./td[7]/div[1]/a/span").InnerHtml.ToString().Trim().Split(" ")[0];
                        var color = item.SelectSingleNode("./td[6]/div[1]/a").InnerHtml;
                        var city = item.SelectSingleNode("./td[9]/div[1]/div[1]/a/span[1]").InnerHtml;
                        var date = item.SelectSingleNode("./td[8]/div[1]/a").InnerHtml;

                        CarTest car = new CarTest();
                        car.Brand = brand;
                        car.Model = model;
                        car.Year = Convert.ToDouble(year);
                        car.City = city;
                        car.Color = color;
                        car.Link = link;
                        car.AdvertDate = date.ToString();
                        if (km.ToString().Contains('.'))
                        {
                            string fullmilage = "";
                            foreach (var part in km.ToString().Split('.'))
                            {
                                fullmilage += part;
                            }
                            car.Milage = Convert.ToDouble(fullmilage);
                        }
                        else
                        {
                            car.Milage = Convert.ToDouble(km);
                        }

                        if (price.ToString().Contains('.'))
                        {
                            string fullprice = "";
                            foreach (var part in price.ToString().Split('.'))
                            {
                                fullprice += part;
                            }
                            car.Price = Convert.ToDouble(fullprice);
                        }
                        else
                        {
                            car.Price = Convert.ToDouble(price);
                        }

                        var listOfExist = _context.Cars.Where(c => c.Link == car.Link).ToList();
                        if (listOfExist.Count() == 0)
                        {

                            _context.CarsTest.Add(car);
                            _context.SaveChanges();
                        }
                        else
                        {
                            existedCounter++;
                            if (existedCounter > 200)
                            {
                                counter = -1001;
                            }
                        }

                        Console.WriteLine(model.ToString() + ": |||||| :" + existedCounter + ": ||||||| :" + date);
                    }

                    counter++;
                    Console.WriteLine(counter.ToString() + " - - - - - - - - - - - - - - - - ");
                }
            }

            ViewBag.Message = "Cars for test has been scraped.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult Scrape()
        {
            for (int i = 0; i <= 14; i++)
            {
                var response = CallUrl($"https://www.arabam.com/ikinci-el/otomobil?days={i}&sort=startedAt.asc").Result;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                int counter = 0;
                int existedCounter = 0;
                string nextPage = "";

                var rows = doc.DocumentNode.SelectNodes("//tr[@class='listing-list-item pr should-hover bg-white']");
                while (counter > -999)
                {
                    if (counter != 0)
                    {
                        nextPage = $"https://www.arabam.com/ikinci-el/otomobil?days={i}&sort=startedAt.asc&;take=50&page={counter}";
                        var res = CallUrl(nextPage);
                        if (!res.IsFaulted)
                        {
                            response = res.Result;
                        }
                        else
                        {
                            Console.WriteLine("Bitti");
                            break;
                        }
                        HtmlDocument newDoc = new HtmlDocument();
                        newDoc.LoadHtml(response);
                        rows = newDoc.DocumentNode.SelectNodes("//tr[@class='listing-list-item pr should-hover bg-white']");
                    }

                    foreach (var item in rows)
                    {
                        var model = item.SelectSingleNode("./td[2]/h3/div").InnerHtml;
                        var brand = model.ToString().Split(" ")[0];
                        var year = item.SelectSingleNode("./td[4]/div[1]/a").InnerHtml;
                        var link = "https://www.arabam.com/" + item.SelectSingleNode("./td[4]/div[1]/a").Attributes[0].Value;
                        var km = item.SelectSingleNode("./td[5]/div[1]/a").InnerHtml;
                        var price = item.SelectSingleNode("./td[7]/div[1]/a/span").InnerHtml.ToString().Trim().Split(" ")[0];
                        var color = item.SelectSingleNode("./td[6]/div[1]/a").InnerHtml;
                        var city = item.SelectSingleNode("./td[9]/div[1]/div[1]/a/span[1]").InnerHtml;
                        var date = item.SelectSingleNode("./td[8]/div[1]/a").InnerHtml;

                        Car car = new Car();
                        car.Brand = brand;
                        car.Model = model;
                        car.Year = Convert.ToDouble(year);
                        car.City = city;
                        car.Color = color;
                        car.Link = link;
                        car.AdvertDate = date.ToString();
                        if (km.ToString().Contains('.'))
                        {
                            string fullmilage = "";
                            foreach (var part in km.ToString().Split('.'))
                            {
                                fullmilage += part;
                            }
                            car.Milage = Convert.ToDouble(fullmilage);
                        }
                        else
                        {
                            car.Milage = Convert.ToDouble(km);
                        }

                        if (price.ToString().Contains('.'))
                        {
                            string fullprice = "";
                            foreach (var part in price.ToString().Split('.'))
                            {
                                fullprice += part;
                            }
                            car.Price = Convert.ToDouble(fullprice);
                        }
                        else
                        {
                            car.Price = Convert.ToDouble(price);
                        }

                        var listOfExist = _context.Cars.Where(c => c.Link == car.Link).ToList();
                        if (listOfExist.Count() == 0)
                        {

                            _context.Cars.Add(car);
                            _context.SaveChanges();
                        }
                        else
                        {
                            existedCounter++;
                            if (existedCounter > 200)
                            {
                                counter = -1001;
                            }
                        }

                        Console.WriteLine(model.ToString() + ": |||||| :" + existedCounter + ": ||||||| :" + date);
                    }

                    counter++;
                    Console.WriteLine(counter.ToString() + " - - - - - - - - - - - - - - - - ");
                }
            }

            ViewBag.Message = "Cars has been scraped.";

            return RedirectToAction(nameof(Profile));
        }

        public static double StrToDouble(string s, double @default)
        {
            double number;
            if (double.TryParse(s, out number))
                return number;
            return @default;
        }

        [Authorize]
        public IActionResult ScrapeDetailsTest()
        {
            var cars = _context.CarsTest.ToList();
            int counter = 0;
            int carcounter = 0;

            foreach (var car in cars)
            {
                Thread.Sleep(100);
                var link = car.Link;
                string res;
                try
                {
                    res = CallUrl(link).Result;
                }
                catch (Exception)
                {
                    continue;
                }


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);

                doc.OptionEmptyCollection = true;

                var rows = doc.DocumentNode.SelectSingleNode("//html/body/div[2]/div[2]/div[3]/div/div[1]/div[1]/div[2]/ul");
                if (rows == null) continue;

                if (rows.SelectSingleNode("./li[8]/span[2]") == null ||
                    rows.SelectSingleNode("./li[9]/span[2]") == null ||
                    rows.SelectSingleNode("./li[10]/span[2]") == null ||
                    rows.SelectSingleNode("./li[11]/span[2]") == null ||
                    rows.SelectSingleNode("./li[12]/span[2]") == null ||
                    rows.SelectSingleNode("./li[14]/span[2]") == null)
                {
                    continue;
                }

                var gear = rows.SelectSingleNode("./li[8]/span[2]").InnerHtml.ToString().Trim();
                var fueltype = rows.SelectSingleNode("./li[9]/span[2]").InnerHtml.ToString().Trim();
                var caseType = rows.SelectSingleNode("./li[10]/span[2]").InnerHtml.ToString().Trim();
                var engVol = rows.SelectSingleNode("./li[11]/span[2]").InnerHtml.ToString().Trim();
                var engPow = rows.SelectSingleNode("./li[12]/span[2]").InnerHtml.ToString().Trim();
                var avgFuelCon = rows.SelectSingleNode("./li[14]/span[2]").InnerHtml.ToString().Trim();

                string fuelCon = avgFuelCon.Replace("lt", "");
                fuelCon = fuelCon.Replace(',', '.').Trim();

                string motGuc = engPow.Replace("hp", "").Trim();
                string motHac = engVol.Replace("cc", "").Trim();

                double dbl_FuelCon = StrToDouble(fuelCon, -999);
                double dbl_motGuc = StrToDouble(motGuc, -999);
                double dbl_motHac = StrToDouble(motHac, -999);

                car.AvgFuelCons = dbl_FuelCon;
                car.EngineVol = dbl_motHac;
                car.EnginePow = dbl_motGuc;
                car.Gear = gear;
                car.Case = caseType;
                car.FuelType = fueltype;


                counter++;

                if (counter % 1000 == 0)
                {
                    Console.WriteLine(counter.ToString());
                    _context.SaveChanges();
                }
            }
            _context.SaveChanges();

            ViewBag.Message = "Cars for test has been scraped for details.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult ScrapeDetails()
        {
            var cars = _context.Cars.ToList();
            int counter = 0;
            int carcounter = 0;

            foreach (var car in cars)
            {
                Thread.Sleep(100);
                var link = car.Link;
                string res;
                try
                {
                    res = CallUrl(link).Result;
                }
                catch (Exception)
                {
                    continue;
                }


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(res);

                doc.OptionEmptyCollection = true;

                var rows = doc.DocumentNode.SelectSingleNode("//html/body/div[2]/div[2]/div[3]/div/div[1]/div[1]/div[2]/ul");
                if (rows == null) continue;

                if (rows.SelectSingleNode("./li[8]/span[2]") == null ||
                    rows.SelectSingleNode("./li[9]/span[2]") == null ||
                    rows.SelectSingleNode("./li[10]/span[2]") == null ||
                    rows.SelectSingleNode("./li[11]/span[2]") == null ||
                    rows.SelectSingleNode("./li[12]/span[2]") == null ||
                    rows.SelectSingleNode("./li[14]/span[2]") == null)
                {
                    continue;
                }

                var gear = rows.SelectSingleNode("./li[8]/span[2]").InnerHtml.ToString().Trim();
                var fueltype = rows.SelectSingleNode("./li[9]/span[2]").InnerHtml.ToString().Trim();
                var caseType = rows.SelectSingleNode("./li[10]/span[2]").InnerHtml.ToString().Trim();
                var engVol = rows.SelectSingleNode("./li[11]/span[2]").InnerHtml.ToString().Trim();
                var engPow = rows.SelectSingleNode("./li[12]/span[2]").InnerHtml.ToString().Trim();
                var avgFuelCon = rows.SelectSingleNode("./li[14]/span[2]").InnerHtml.ToString().Trim();

                string fuelCon = avgFuelCon.Replace("lt", "");
                fuelCon = fuelCon.Replace(',', '.').Trim();

                string motGuc = engPow.Replace("hp", "").Trim();
                string motHac = engVol.Replace("cc", "").Trim();

                double dbl_FuelCon = StrToDouble(fuelCon, -999);
                double dbl_motGuc = StrToDouble(motGuc, -999);
                double dbl_motHac = StrToDouble(motHac, -999);

                car.AvgFuelCons = dbl_FuelCon;
                car.EngineVol = dbl_motHac;
                car.EnginePow = dbl_motGuc;
                car.Gear = gear;
                car.Case = caseType;
                car.FuelType = fueltype;


                counter++;

                if (counter % 1000 == 0)
                {
                    Console.WriteLine(counter.ToString());
                    _context.SaveChanges();
                }
            }
            _context.SaveChanges();

            ViewBag.Message = "Cars have been scraped for details.";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult TrainPrice()
        {
            var cars = _context.Cars;
            MLContext mLContext = new MLContext();
            List<ModelInput> miList = new List<ModelInput>();

            foreach (Car car in cars)
            {
                miList.Add(PricingModel.ReturnModelInput(car));
            }

            IDataView dw = mLContext.Data.LoadFromEnumerable(miList);
            var model = PricingModel.TrainPipeline(mLContext, dw, 475, 2, 32768, 600, 0.19f, 0.75f);

            mLContext.Model.Save(model, dw.Schema, "Modella.zip");

            //var car1 = _context.Cars.Find(1);
            //var x  = PricingModel.Predict(ReturnModelInput(car1));

            ViewBag.Message = "Model is trained!";

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult FixModelNames() {

            var cars = _context.Cars;
            var testCars = _context.CarsTest;
            Dictionary<string, int> modelNames = new Dictionary<string, int>();
            Dictionary<string, int> modelNames2 = new Dictionary<string, int>();

            foreach (Car car in cars)
            {
                string modelNam = car.Model;
                var x = modelNam.Split(" ");

                if(x.Length <= 1)
                {
                    continue;
                }

                string name = "";

                for (int i = 0; i < 3; i++) {
                    if (x[i] == car.Brand || x[i].Contains(".")) {
                        continue;
                    }
                    name += x[i];
                }

                if (!modelNames.ContainsKey(name))
                {

                    modelNames.Add(name, 1);
                }
                else {
                    modelNames[name]++;
                }

                car.Model = name;
            }

            

            foreach (CarTest car in testCars)
            {
                string modelNam = car.Model;
                var x = modelNam.Split(" ");

                if (x.Length <= 1)
                {
                    continue;
                }

                string name = "";

                for (int i = 0; i < 3; i++)
                {
                    if (x[i] == car.Brand || x[i].Contains("."))
                    {
                        continue;
                    }
                    name += x[i];
                }

                if (!modelNames2.ContainsKey(name))
                {

                    modelNames2.Add(name, 1);
                }
                else
                {
                    modelNames2[name]++;
                }

                car.Model = name;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult EvaluatePrice()
        {
            var cars = _context.CarsTest.ToList();
            MLContext mLContext = new MLContext();
            List<ModelInput> miList = new List<ModelInput>();

            foreach (CarTest car in cars)
            {
                miList.Add(PricingModel.ReturnModelInputForTest(car));
            }
            /*IDataView dw = mLContext.Data.LoadFromEnumerable(miList);

            PricingModel PM = new PricingModel();
            PM.Evaluate(mLContext, dw);*/

            string avgDifference = PricingModel.EvaluationFunction2(mLContext, miList);

            ViewBag.Message = "Model is evaluated! Average difference is: " + avgDifference;

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult EvaluateBuiltIn()
        {
            var cars = _context.CarsTest.ToList();
            MLContext mLContext = new MLContext();
            List<ModelInput> miList = new List<ModelInput>();

            foreach (CarTest car in cars)
            {
                miList.Add(PricingModel.ReturnModelInputForTest(car));
            }

            IDataView dw = mLContext.Data.LoadFromEnumerable(miList);

            PricingModel PM = new PricingModel();
            var listOfMessages = PM.Evaluate(mLContext, dw);

            //PricingModel.MyEvaluationFunction(mLContext, miList);
            ViewBag.Message = "Model is evaluated!" + listOfMessages[0] + "\r\n" + listOfMessages[1];
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public IActionResult EvaluateFeatures()
        {
            var cars = _context.Cars.ToList();

            var top10Models = _context.Cars.GroupBy(i => i.Model)
                     .Where(x => x.Count() > 1).OrderByDescending(x => x.Count())
                     .Select(val => val.Key).Take(10).ToList();



            var exampleCars = exampleCarsForFeatureImportance(cars, top10Models);

            var featuresAndValues = GetFeaturePriceDictionary(exampleCars);

            foreach (var item in _context.FeatureTests)
            {
                _context.FeatureTests.Remove(item);
            }

            _context.SaveChanges();

            _context.FeatureTests.AddRange(featuresAndValues);

            _context.SaveChanges();

            ViewBag.Message = "Features are evaluated!";

            return RedirectToAction(nameof(Profile));
        }

        public List<Car> exampleCarsForFeatureImportance(List<Car> cars, List<string> top10Models) {

            List<Car> result = new List<Car>();
            

            foreach (var model in top10Models) { 
                
                var carsWithModel = cars.Where(x=>x.Model == model).ToList();

                var MostRepeatedGear = carsWithModel.GroupBy(q => q.Gear)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedYear = carsWithModel.GroupBy(q => q.Year)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedAvgFuel = carsWithModel.GroupBy(q => q.AvgFuelCons)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedBrand = carsWithModel.GroupBy(q => q.Brand)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedCase = carsWithModel.GroupBy(q => q.Case)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedCity = carsWithModel.GroupBy(q => q.City)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedColor = carsWithModel.GroupBy(q => q.Color)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedEnginePow = carsWithModel.GroupBy(q => q.EnginePow)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedEngineVol = carsWithModel.GroupBy(q => q.EngineVol)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedFuelType = carsWithModel.GroupBy(q => q.FuelType)
                                    .OrderByDescending(gp => gp.Count())
                                    .Take(1)
                                    .Select(g => g.Key).ToList().First();

                var MostRepeatedMilage = carsWithModel.Select(q => q.Milage).Average();


                var MostRepeatedPrice = carsWithModel.Select(q => q.Price).Average();

                Car exampleCar = new Car();
                exampleCar.Model = model;
                exampleCar.Year = MostRepeatedYear;
                exampleCar.Brand = MostRepeatedBrand;
                exampleCar.Price = MostRepeatedPrice;
                exampleCar.City = MostRepeatedCity;
                exampleCar.Gear = MostRepeatedGear;
                exampleCar.Milage = MostRepeatedMilage;
                exampleCar.FuelType = MostRepeatedFuelType;
                exampleCar.EnginePow = MostRepeatedEnginePow;
                exampleCar.EngineVol = MostRepeatedEngineVol;
                exampleCar.Color = MostRepeatedColor;
                exampleCar.Case = MostRepeatedCase;
                exampleCar.AvgFuelCons = MostRepeatedAvgFuel;

                result.Add(exampleCar);

            }

            return result;
        }

        public List<FeatureValueOutput> GetFeaturePriceDictionary(List<Car> cars){

            //Gear, FuelType, Color, City, Case

            List<string> features = new List<string>() { "Gear", "FuelType", "Color", "City", "Case", "AvgFuelCons", "Model", "Brand" };

            List<FeatureValueOutput> featureValueOutputs = new List<FeatureValueOutput> ();

            foreach (var car in cars)
            {
                var outputForCar = PricingModel.Predict(PricingModel.ReturnModelInput(car));
                foreach (var feature in features) 
                {

                    List<string> possibleValues = new List<string> ();
                    

                    switch (feature) {
                        case "Gear":
                            possibleValues = _context.Cars.Select(x => x.Gear).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.Gear = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);



                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput)/2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "FuelType":
                            possibleValues = _context.Cars.Select(x => x.FuelType).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.FuelType = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "Color":
                            possibleValues = _context.Cars.Select(x => x.Color).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.Color = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "City":
                            possibleValues = _context.Cars.Select(x => x.City).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.City = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "Case":
                            possibleValues = _context.Cars.Select(x => x.Case).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.Case = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "Model":
                            possibleValues = _context.Cars.Select(x => x.Model).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.Model = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "Brand":
                            possibleValues = _context.Cars.Select(x => x.Brand).Distinct().ToList();
                            foreach (var possibleValue in possibleValues)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.Brand = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue;
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                        case "AvgFuelCons":
                            var possibleValuesForFuel = new List<double>() { 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 15.0, 20.0};
                            foreach (var possibleValue in possibleValuesForFuel)
                            {
                                FeatureValueOutput fvo = new FeatureValueOutput();
                                car.AvgFuelCons = possibleValue;
                                var output = PricingModel.Predict(PricingModel.ReturnModelInput(car));

                                float differenceBetweenOriginalOutput = (float)(output.Score - outputForCar.Score);
                                float differenceBetweenOriginalPrice = (float)(output.Score - car.Price);

                                fvo.Feature = feature;
                                fvo.Value = possibleValue.ToString();
                                fvo.Output = (float)((differenceBetweenOriginalPrice + differenceBetweenOriginalOutput) / 2);

                                featureValueOutputs.Add(fvo);
                            }
                            break;
                    }

                }

            }

            return featureValueOutputs;
        }

        public List<List<T>> SplitList<T>(List<T> me, int size = 50)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < me.Count; i += size)
                list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
            return list;
        }

        public void CrossValidationForModel(int nol, int mecpl, int not, int mbcpf, float lr, float fef, string modelname)
        {
            var cars = _context.Cars.ToList();

            int split = 8;
            int size = cars.Count / split;

            var carsForCV = SplitList(cars, size);

            //RSquared Score: {messagelist[0]} \r\n MeanSq Error Score: {messagelist[1]} \r\n Root Mean Squared Error:

            double rSqScore = 0;
            double MeanSqError = 0;
            double RootMeanSquaredError = 0;


            List<Car> train = new List<Car>();
            List<Car> test = new List<Car>(); 

            for(int j = 0; j<split; j++)
            {
                test = carsForCV[j];
                for(int i = 0; i < split; i++)
                {
                    if (i == j) { continue; }
                    train.AddRange(carsForCV[i]);
                }

                var metrics = TrainModelEvaluateAndSave(modelname, train, nol, mecpl, not, mbcpf, lr, fef, test);

                rSqScore += metrics[0];
                MeanSqError += metrics[1];
                RootMeanSquaredError += metrics[2];    

                train.RemoveAll(x=> x != null);
                test.RemoveAll(x => x != null);
            }

            Console.WriteLine($"For model {modelname}: \r\n RSquared Score: {rSqScore/split} \r\n MeanSq Error Score: {MeanSqError / split} \r\n Root Mean Squared Error: {RootMeanSquaredError / split}");
            string toAppend = $"For model {modelname}: \r\n RSquared Score: {rSqScore / split} \r\n MeanSq Error Score: {MeanSqError / split} \r\n Root Mean Squared Error: {RootMeanSquaredError / split}";
            System.IO.File.AppendAllText("modelComparison2s.txt", toAppend);

        }

        public IActionResult CrossValidation()
        {
            /** MODEL A 
             * NumberOfLeaves = 100;
             * MinimumExampleCountPerLeaf = 10;
             * NumberOfTrees = 100;
             * MaximumBinCountPerFeature = 100;
             * LearningRate = 0.15;
             * FeatureFraction = 0.7;
             * **/

            CrossValidationForModel(100, 10, 100, 100, 0.15f, 0.7f, "ModelA.zip");



            /** MODEL B
             * NumberOfLeaves = 200;
             * MinimumExampleCountPerLeaf = 5;
             * NumberOfTrees = 1000;
             * MaximumBinCountPerFeature = 300;
             * LearningRate = 0.1;
             * FeatureFraction = 0.8;
             *  **/

            CrossValidationForModel(200, 5, 1000, 300, 0.1f, 0.8f, "ModelB.zip");
            //TrainModelEvaluateAndSave("ModelB.zip", cars.ToList(), 200, 5, 1000, 300, 0.1f, 0.8f);

            /** MODEL C
             * NumberOfLeaves = 300;
             * MinimumExampleCountPerLeaf = 20;
             * NumberOfTrees = 5000;
             * MaximumBinCountPerFeature = 1000;
             * LearningRate = 0.2f;
             * FeatureFraction = 0.7F;
             * **/

            CrossValidationForModel(300, 20, 5000, 1000, 0.2f, 0.7F, "ModelC.zip");
            //TrainModelEvaluateAndSave("ModelC.zip", cars.ToList(), 300, 20, 5000, 1000, 0.2f, 0.7F);

            /** MODEL D
             * NumberOfLeaves = 600;
             * MinimumExampleCountPerLeaf = 2;
             * NumberOfTrees = 32768;
             * MaximumBinCountPerFeature = 600;
             * LearningRate =  0.2f;
             * FeatureFraction =  0.75f;
             * **/

            CrossValidationForModel(600, 2, 32768, 600, 0.2f, 0.75f, "ModelD.zip");

            //TrainModelEvaluateAndSave("ModelD.zip", cars.ToList(), 600, 2, 32768, 600, 0.2f, 0.75f);

            /** MODEL E
             * NumberOfLeaves = 1000;
             * MinimumExampleCountPerLeaf = 10;
             * NumberOfTrees = 20000;   
             * MaximumBinCountPerFeature = 500;
             * LearningRate = 0.2f;
             * FeatureFraction = 0.8f;
             * **/

            CrossValidationForModel(1000, 10, 20000, 500, 0.2f, 0.8f, "ModelE.zip");
            //TrainModelEvaluateAndSave("ModelE.zip", cars.ToList(), 1000, 10, 20000, 500, 0.2f, 0.8f);

            /** MODEL F
             * NumberOfLeaves = 485;
             * MinimumExampleCountPerLeaf = 2;
             * NumberOfTrees = 32768;
             * MaximumBinCountPerFeature = 600;
             * LearningRate = 0.2f;
             * FeatureFraction = 0.85f;
             * **/

            CrossValidationForModel(485, 2, 32768, 600, 0.2f, 0.85f, "ModelF.zip");
            //TrainModelEvaluateAndSave("ModelF.zip", cars.ToList(), 485, 2, 32768, 600, 0.2f, 0.85f);

            /** MODEL G
             * NumberOfLeaves = 500;
             * MinimumExampleCountPerLeaf = 4;
             * NumberOfTrees = 6000;
             * MaximumBinCountPerFeature = 750;
             * LearningRate = 0.14f;
             * FeatureFraction = 0.9f;
             * **/

            CrossValidationForModel(500, 4, 6000, 750, 0.14f, 0.9f, "ModelG.zip");
            //TrainModelEvaluateAndSave("ModelG.zip", cars.ToList(), 500, 4, 6000, 750, 0.14f, 0.9f);

            /** MODEL H
             * NumberOfLeaves = 385;
             * MinimumExampleCountPerLeaf = 2;
             * NumberOfTrees = 2500;
             * MaximumBinCountPerFeature = 100;
             * LearningRate = 0.25f;
             * FeatureFraction = 0.68f;
             * **/

            CrossValidationForModel(385, 2, 2500, 100, 0.25f, 0.68f, "ModelH.zip");
            //TrainModelEvaluateAndSave("ModelH.zip", cars.ToList(), 385, 2, 2500, 100, 0.25f, 0.68f);

            /** MODEL I
             * NumberOfLeaves = 475;
             * MinimumExampleCountPerLeaf = 2;
             * NumberOfTrees = 32768;
             * MaximumBinCountPerFeature = 600;
             * LearningRate = 0.19f;
             * FeatureFraction = 0.75f;
             * **/

            CrossValidationForModel(475, 2, 32768, 600, 0.19f, 0.75f, "ModelI.zip");
            //TrainModelEvaluateAndSave("ModelI.zip", cars.ToList(), 475, 2, 32768, 600, 0.19f, 0.75f);

            /** MODEL J
             * NumberOfLeaves = 485;
             * MinimumExampleCountPerLeaf = 2;
             * NumberOfTrees = 400;
             * MaximumBinCountPerFeature = 600;
             * LearningRate = 0.2f;
             * FeatureFraction = 0.75f;
             * **/

            CrossValidationForModel(485, 2, 400, 600, 0.2f, 0.75f, "ModelJ.zip");
            //TrainModelEvaluateAndSave("ModelJ.zip", cars.ToList(), 485, 2, 400, 600, 0.2f, 0.75f);

            return RedirectToAction(nameof(Profile));
        }

        private List<double> TrainModelEvaluateAndSave(string modelName, List<Car> carList, int NumberOfLeaves, int MinimumExampleCountPerLeaf, int NumberOfTrees, int MaximumBinCountPerFeature, float LearningRate, float FeatureFraction, List<Car> testData)
        {
            MLContext _MLContext = new MLContext();
            List<ModelInput> modelInputs = new List<ModelInput>();
            List<ModelInput> modelInputsTest = new List<ModelInput>();
            List<Car> carsTest = testData;


            foreach (Car car in carList)
            {
                modelInputs.Add(PricingModel.ReturnModelInput(car));
            }

            foreach (Car car in carsTest)
            {
                modelInputsTest.Add(PricingModel.ReturnModelInput(car));
            }


            IDataView dwTrain = _MLContext.Data.LoadFromEnumerable(modelInputs);
            IDataView dwTest = _MLContext.Data.LoadFromEnumerable(modelInputsTest);

            PricingModelForValidation pmfv = new PricingModelForValidation(modelName, 
                NumberOfLeaves, 
                MinimumExampleCountPerLeaf, 
                NumberOfTrees, 
                MaximumBinCountPerFeature, 
                LearningRate, 
                FeatureFraction, 
                _MLContext);

            var metrics = pmfv.Run(dwTrain, dwTest);

            return metrics;
        }
    }
}