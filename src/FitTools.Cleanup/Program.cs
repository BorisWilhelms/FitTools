using Google.Apis.Auth.OAuth2;
using Google.Apis.Fitness.v1;
using Google.Apis.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FitTools.Cleanup
{
    class Program
    {
        private const string VeryfitWeight = "raw:com.google.weight:com.veryfit2hr.second:";

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

            var service = new FitnessService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "Fit Cleanup" });
            var dataSource = service.Users.DataSources.DataPointChanges.List("me", VeryfitWeight).Execute();

            Console.WriteLine($"Got {dataSource.InsertedDataPoint.Count} points");


            var startTime = dataSource.InsertedDataPoint.Min(p => p.StartTimeNanos);
            var endTime = dataSource.InsertedDataPoint.Max(p => p.EndTimeNanos);
            if (startTime.HasValue && endTime.HasValue)
            {
                Console.WriteLine("Deleting points");
                var response = service.Users.DataSources.Datasets.Delete("me", VeryfitWeight, $"{startTime.Value}-{endTime.Value}").Execute();
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
