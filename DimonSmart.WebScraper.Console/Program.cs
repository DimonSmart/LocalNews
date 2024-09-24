using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DimonSmart.WebScraper.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                await Host.CreateDefaultBuilder(args)
                    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
                    .UseSerilog()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        config.AddUserSecrets<Program>();
                        config.AddEnvironmentVariables();
                        config.AddCommandLine(args);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        var configuration = hostContext.Configuration;

                        services
                            .AddHostedService<ConsoleHostedService>()
                            .AddSingleton<IPageDownloader, PageDownloader>()
                            .AddSingleton<ILinkExtractor, LinkExtractor>()
                            .AddSingleton<IPageStorage, PageStorage>()
                            .AddSingleton<IUrlQueueManager, UrlQueueManager>()
                            .AddSingleton<WebScraper>()
                            .AddSingleton<IUrlRepository, FileUrlRepository>() // Register IUrlRepository
                            .Configure<WebScraperSettings>(configuration.GetSection("WebScraperSettings"))
                            .Configure<StorageSettings>(configuration.GetSection("StorageSettings"));

                        // Register Serilog.ILogger
                        services.AddSingleton(Log.Logger);
                    })
                    .RunConsoleAsync();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }

    public class ConsoleHostedService : IHostedService
    {
        private readonly WebScraper _webScraper;

        public ConsoleHostedService(WebScraper webScraper)
        {
            _webScraper = webScraper;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var downloadRequests = new List<DownloadRequest>
                            {
                                new DownloadRequest("https://visita.malaga.eu/en/", 1)
                            };

            await _webScraper.ScrapAsync(downloadRequests);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
