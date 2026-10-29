using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace HotelBooking.E2ETests
{
    /// <summary>
    /// Starts the MVC app before the tests and stops it after them.
    /// The app reseeds its database on every start, so each test run
    /// begins with the same hotel: 3 rooms, all booked day 4 – day 18.
    /// </summary>
    public class MvcAppFixture : IAsyncLifetime
    {
        public const string BaseUrl = "http://localhost:5000";

        private Process app;

        public async ValueTask InitializeAsync()
        {
            app = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project HotelBooking.Mvc --launch-profile http",
                WorkingDirectory = FindSolutionFolder(),
                UseShellExecute = false
            });

            await WaitUntilAppAnswers();
        }

        public ValueTask DisposeAsync()
        {
            app.Kill(entireProcessTree: true);
            app.Dispose();
            return ValueTask.CompletedTask;
        }

        // The first start also builds the app, so allow up to 2 minutes.
        private static async Task WaitUntilAppAnswers()
        {
            using var http = new HttpClient();
            DateTime giveUp = DateTime.Now.AddMinutes(2);

            while (DateTime.Now < giveUp)
            {
                try
                {
                    var response = await http.GetAsync(BaseUrl + "/Bookings");
                    if (response.IsSuccessStatusCode)
                        return;
                }
                catch (HttpRequestException)
                {
                    // Not listening yet
                }
                await Task.Delay(500);
            }
            throw new TimeoutException($"The MVC app did not answer on {BaseUrl}.");
        }

        // Walk up from bin/Debug/... until we find HotelBooking.sln
        private static string FindSolutionFolder()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (!File.Exists(Path.Combine(folder.FullName, "HotelBooking.sln")))
                folder = folder.Parent;
            return folder.FullName;
        }
    }
}
