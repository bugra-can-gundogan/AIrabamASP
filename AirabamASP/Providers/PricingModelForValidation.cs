using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;

namespace AirabamASP.Providers
{
    public class PricingModelForValidation
    {
        private static string ModelPath;
        private static MLContext _MLContext;
        private static int NumberOfLeaves;
        private static int MinimumExampleCountPerLeaf; 
        private static int NumberOfTrees; 
        private static int MaximumBinCountPerFeature; 
        private static float LearningRate; 
        private static float FeatureFraction;

        public PricingModelForValidation(string modelPath, int numberOfLeaves, int minimumExampleCountPerLeaf, int numberOfTrees, int maximumBinCountPerFeature, float learningRate, float featureFraction, MLContext mLContext)
        {
            ModelPath = modelPath;
            NumberOfLeaves = numberOfLeaves;
            MinimumExampleCountPerLeaf = minimumExampleCountPerLeaf;
            NumberOfTrees = numberOfTrees;
            MaximumBinCountPerFeature = maximumBinCountPerFeature;
            LearningRate = learningRate;
            FeatureFraction = featureFraction;
            _MLContext = mLContext;
        }

        public List<double> Run(IDataView trainData, IDataView testData)
        {

            var model = TrainPipeline(_MLContext, trainData);
            var messagelist = Evaluate(_MLContext, testData,model);

            _MLContext.Model.Save(model, trainData.Schema, ModelPath);

            return messagelist;
        }

        private static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(ModelPath, out var _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }

        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
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
                                    .Append(mlContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options()
                                    {
                                        NumberOfLeaves = NumberOfLeaves,
                                        MinimumExampleCountPerLeaf = MinimumExampleCountPerLeaf,
                                        NumberOfTrees = NumberOfTrees,
                                        MaximumBinCountPerFeature = MaximumBinCountPerFeature,
                                        LearningRate = LearningRate,
                                        FeatureFraction = FeatureFraction,
                                        LabelColumnName = @"Price",
                                        FeatureColumnName = @"Features"
                                    }));

            return pipeline;
        }

        public static ITransformer TrainPipeline(MLContext context, IDataView trainData)
        {
            var pipeline = BuildPipeline(context);
            var model = pipeline.Fit(trainData);

            return model;
        }

        public static ModelOutput Predict(ModelInput input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        public static List<double> Evaluate(MLContext mlContext, IDataView testData, ITransformer trainedModel)
        {
            var list = new List<double>();
            ITransformer model = trainedModel;
            var predictions = model.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, "Price", "Score");

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       MeanSq Error Score:      {metrics.MeanSquaredError:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");

            list.Add(metrics.RSquared);
            list.Add(metrics.MeanSquaredError);
            list.Add(metrics.RootMeanSquaredError);

            return list;
        }

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
    }
}
