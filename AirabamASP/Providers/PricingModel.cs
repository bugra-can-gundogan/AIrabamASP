using AirabamASP.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace AirabamASP.Providers
{
    public class PricingModel
    {
        public class ModelInput
        {
            [ColumnName(@"Id")]
            public float Id { get; set; }

            [ColumnName(@"AdvertDate")]
            public string AdvertDate { get; set; }

            [ColumnName(@"Brand")]
            public string Brand { get; set; }

            [ColumnName(@"City")]
            public string City { get; set; }

            [ColumnName(@"Color")]
            public string Color { get; set; }

            [ColumnName(@"Link")]
            public string Link { get; set; }

            [ColumnName(@"Milage")]
            public float Milage { get; set; }

            [ColumnName(@"Model")]
            public string Model { get; set; }

            [ColumnName(@"Price")]
            public float Price { get; set; }

            [ColumnName(@"Year")]
            public float Year { get; set; }

            [ColumnName(@"AvgFuelCons")]
            public float AvgFuelCons { get; set; }

            [ColumnName(@"Case")]
            public string Case { get; set; }

            [ColumnName(@"EnginePow")]
            public float EnginePow { get; set; }

            [ColumnName(@"EngineVol")]
            public float EngineVol { get; set; }

            [ColumnName(@"FuelType")]
            public string FuelType { get; set; }

            [ColumnName(@"Gear")]
            public string Gear { get; set; }

        }

        public class ModelOutput
        {
            public float Score { get; set; }
        }

        public static string MLNetModelPath = Path.GetFullPath("Modella.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }

        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext, int NumberOfLeaves, int MinimumExampleCountPerLeaf,
            int NumberOfTrees, int MaximumBinCountPerFeature, float LearningRate, float FeatureFraction)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] {
                                        new InputOutputColumnPair(@"Brand", @"Brand"),
                                        new InputOutputColumnPair(@"City", @"City"),
                                        new InputOutputColumnPair(@"Color", @"Color"),
                                        new InputOutputColumnPair(@"Model", @"Model"),
                                        new InputOutputColumnPair(@"Case", @"Case"),
                                        new InputOutputColumnPair(@"FuelType", @"FuelType"),
                                        new InputOutputColumnPair(@"Gear", @"Gear") })
                                    .Append(mlContext.Transforms.ReplaceMissingValues(new[] {
                                        new InputOutputColumnPair(@"Milage", @"Milage"),
                                        new InputOutputColumnPair(@"Year", @"Year"),
                                        new InputOutputColumnPair(@"AvgFuelCons", @"AvgFuelCons"),
                                        new InputOutputColumnPair(@"EnginePow", @"EnginePow"),
                                        new InputOutputColumnPair(@"EngineVol", @"EngineVol") }))
                                    .Append(mlContext.Transforms.Text.FeaturizeText(@"AdvertDate", @"AdvertDate"))
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new[] {
                                        @"Brand",
                                        @"City",
                                        @"Color",
                                        @"Model",
                                        @"Case",
                                        @"FuelType",
                                        @"Gear",
                                        @"Milage",
                                        @"Year",
                                        @"AvgFuelCons",
                                        @"EnginePow",
                                        @"EngineVol",
                                        @"AdvertDate" }))
                                    .Append(mlContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options() { 
                                        NumberOfLeaves = NumberOfLeaves, 
                                        MinimumExampleCountPerLeaf = MinimumExampleCountPerLeaf, 
                                        NumberOfTrees = NumberOfTrees, 
                                        MaximumBinCountPerFeature = MaximumBinCountPerFeature, 
                                        LearningRate = LearningRate, 
                                        FeatureFraction = FeatureFraction, 
                                        LabelColumnName = @"Price", 
                                        FeatureColumnName = @"Features" }));

            return pipeline;
        }


        //485, 2, 32768, 600, 0.193775467159845F, 0.744979057114779F

        public static ITransformer TrainPipeline(MLContext context, IDataView trainData, int NumberOfLeaves, int MinimumExampleCountPerLeaf,
            int NumberOfTrees, int MaximumBinCountPerFeature, float LearningRate, float FeatureFraction)
        {
            var pipeline = BuildPipeline(context, NumberOfLeaves, MinimumExampleCountPerLeaf, NumberOfTrees, MaximumBinCountPerFeature, LearningRate, FeatureFraction);
            var model = pipeline.Fit(trainData);

            return model;
        }


        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        public List<String> Evaluate(MLContext mlContext, IDataView testData)
        {
            var list = new List<String>();
            ITransformer model = mlContext.Model.Load(MLNetModelPath, out var _);
            var predictions = model.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, "Price", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       MeanSq Error Score:      {metrics.MeanSquaredError:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
            list.Add($"*       RSquared Score:      {metrics.RSquared:0.##}");
            list.Add($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
            return list;
        }

        public static ModelInput ReturnModelInput(Car car)
        {
            ModelInput model = new ModelInput
            {
                Brand = car.Brand,
                Model = car.Model,
                City = car.City,
                Color = car.Color,
                Year = (float)car.Year,
                Milage = (float)car.Milage,
                Price = (float)car.Price,
                AdvertDate = car.AdvertDate,
                Gear = car.Gear,
                Case = car.Case,
                AvgFuelCons = (float)car.AvgFuelCons,
                FuelType = car.FuelType,
                EnginePow = (float)car.EnginePow,
                EngineVol = (float)car.EngineVol
            };

            return model;
        }

        public static ModelInput ReturnModelInputForTest(CarTest car)
        {
            ModelInput model = new ModelInput
            {
                Brand = car.Brand,
                Model = car.Model,
                City = car.City,
                Color = car.Color,
                Year = (float)car.Year,
                Milage = (float)car.Milage,
                Price = (float)car.Price,
                AdvertDate = car.AdvertDate,
                Gear = car.Gear,
                Case = car.Case,
                AvgFuelCons = (float)car.AvgFuelCons,
                FuelType = car.FuelType,
                EnginePow = (float)car.EnginePow,
                EngineVol = (float)car.EngineVol
            };

            return model;
        }

        public static string EvaluationFunction2(MLContext mLContext, List<ModelInput> carsList) {

            ITransformer model = mLContext.Model.Load(MLNetModelPath, out var _);

            List<Tuple<float, float, float>> tuples = new List<Tuple<float, float, float>>();

            float sumOfDifferences = 0;
            float averageDifference = 0;
            int lessCounter = 0;
            int biggerCounter = 0;

            foreach (var car in carsList) {
                float score = Predict(car).Score;
                float price = car.Price;

                //PercentDifferenceFormula = (|n1-n2|/((n1+n2)/2)) * 100
                float difference = Math.Abs(score - price);
                float divident = (score + price) / 2;

                if (score > price)
                {
                    biggerCounter++;
                }else if(score< price)
                {
                    lessCounter++;
                }


                float percentDifference = (difference / divident) * 100;

                averageDifference += Math.Abs(score - price);

                sumOfDifferences += percentDifference;

                tuples.Add(new Tuple<float, float,float>(price, score, percentDifference)); 
            }

            foreach (var tuple in tuples) { 
                Console.WriteLine(tuple.Item1 + " | " + tuple.Item2 + " | " + " difference in price : " + (Math.Abs(tuple.Item1 - tuple.Item2) + " difference in percentage: " + tuple.Item3));
            }

            Console.WriteLine("Average Percentage Difference: " + (sumOfDifferences / carsList.Count));
            Console.WriteLine("Average Difference: " + (averageDifference / carsList.Count));
            Console.WriteLine($"AIrabam overestimated prices of {biggerCounter} cars.");
            Console.WriteLine($"AIrabam underestimated prices of {lessCounter} cars.");

            return ("Average Difference: " + (sumOfDifferences/carsList.Count));

        }
        
    }
}
