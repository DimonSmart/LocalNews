using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
                            .AddTransient<IPageDownloader, PageDownloader>()
                            .AddSingleton<ILinkExtractor, LinkExtractor>()
                            .AddSingleton<IPageStorage, PageStorage>()
                            .AddSingleton<IUrlQueueManager, UrlQueueManager>()
                            .AddSingleton<WebScraper>()
                            .AddSingleton<IUrlRepository, FileUrlRepository>()
                            .AddDbContext<AppDbContext>(options => options.UseSqlite(configuration.GetConnectionString("FileStorageDb"), b => b.MigrationsAssembly("DimonSmart.WebScraper")))
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
}
