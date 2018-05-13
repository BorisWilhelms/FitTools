using Google.Apis.Auth.OAuth2;
using Google.Apis.Fitness.v1;
using Google.Apis.Fitness.v1.Data;
using Google.Apis.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FitTools
{
    class Program
    {
        private const string WeightUserInput = "raw:com.google.weight:com.google.android.apps.fitness:user_input";

        static async Task Main(string[] args)
        {
            UserCredential credential;
            using (var stream = new FileStream("client_id.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { FitnessService.Scope.FitnessBodyWrite },
                    "user", CancellationToken.None);
            }

            var service = new FitnessService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "Fit Importer" });
            var points = JsonConvert.DeserializeObject<List<DataPoint>>(File.ReadAllText("import.json"));
            var dataSet = new Dataset()
            {
                DataSourceId = WeightUserInput,
                MaxEndTimeNs = points.Max(p => p.EndTimeNanos.Value),
                MinStartTimeNs = points.Min(p => p.StartTimeNanos.Value),
                Point = points
            };
            Console.WriteLine($"Importing {dataSet.Point.Count} data points");
            var response = service
                .Users
                .DataSources
                .Datasets
                .Patch(dataSet, "me", WeightUserInput, $"{points.Min(p => p.StartTimeNanos.Value)}-{points.Max(p => p.EndTimeNanos.Value)}")
                .Execute();

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
