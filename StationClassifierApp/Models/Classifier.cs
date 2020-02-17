using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

using Xam.Plugins.OnDeviceCustomVision;

namespace StationCustomVisionApp
{
    static class Classifier
    {

        /*static readonly Guid CV_PROJECT_ID = Guid.Parse("ee365168-f1a3-4d3a-a580-d5205ff96c56");
        static readonly string CV_PREDICTION_KEY = "864f701d1c1e4c668117289b19b26424";
        static readonly string CV_PREDICTION_ENDPOINT = "https://eastus.api.cognitive.microsoft.com";
        static readonly string CV_ITERATION_NAME = "Iteration4";

        public static CustomVisionPredictionClient ClassifierClient { get; } = new CustomVisionPredictionClient()
        {
            ApiKey = CV_PREDICTION_KEY,
            Endpoint = CV_PREDICTION_ENDPOINT
        };
        public static string GetPredictions(Stream imgStream)
        {
            var result = ClassifierClient.ClassifyImage(CV_PROJECT_ID, CV_ITERATION_NAME, imgStream);

            string output = "";

            foreach (PredictionModel prediction in result.Predictions)
                output += $"{prediction.TagName}: {prediction.Probability:P1}\n";

            return output;
        }
*/

        public async static Task<List<ImageClassification>> GetPredictionsLocal(Stream imgStream)
        {
            IReadOnlyList<ImageClassification> classifications;

            try { classifications = await CrossImageClassifier.Current.ClassifyImage(imgStream); }
            catch(Exception e) { throw e.InnerException; }

            return new List<ImageClassification>(classifications);
        }
    }

}
