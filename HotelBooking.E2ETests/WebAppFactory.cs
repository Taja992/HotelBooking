using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace HotelBooking.E2ETests
{
    /// <summary>
    /// Starts the MVC app as a real process so ChromeDriver can reach it over HTTP.
    /// The app runs with ASPNETCORE_ENVIRONMENT=Development, which causes DbInitializer
    /// to delete and re-seed the database — giving every test run a clean starting state.
    /// </summary>
    public class WebAppFactory : IDisposable
    {
        private readonly Process _process;

        public Uri BaseAddress { get; } = new Uri("http://127.0.0.1:5050");

        public WebAppFactory()
        {
            var mvcDll = Path.Combine(AppContext.BaseDirectory, "HotelBooking.Mvc.dll");

            _process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet")
                {
                    Arguments       = $"\"{mvcDll}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError  = false,
                    WorkingDirectory = AppContext.BaseDirectory,
                }
            };
            _process.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
            _process.StartInfo.Environment["ASPNETCORE_URLS"]        = BaseAddress.ToString();

            _process.Start();
            WaitForReady();
        }

        private void WaitForReady()
        {
            using var client = new HttpClient();
            var deadline = DateTime.Now.AddSeconds(30);
            while (DateTime.Now < deadline)
            {
                try
                {
                    client.GetAsync(BaseAddress).GetAwaiter().GetResult();
                    return;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
            throw new TimeoutException("MVC app failed to start within 30 seconds");
        }

        public void Dispose()
        {
            try { _process?.Kill(entireProcessTree: true); } catch { }
            _process?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
