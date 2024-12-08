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
                var host = Host.CreateDefaultBuilder(args)
                    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
                    .UseSerilog()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        config.AddUserSecrets<Program>();
                        config.AddEnvironmentVariables();
                        config.AddCommandLine(args);
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog();
                        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        var configuration = hostContext.Configuration;

                        services
                            .AddSingleton<IMainContentExtractor, MainContentExtractor>()
                            .AddSingleton<IHostedService, ConsoleHostedService>()
                            .AddTransient<IPageDownloader, PageDownloader>()
                            .AddSingleton<ILinkExtractor, LinkExtractor>()
                            .AddSingleton<IUrlQueueManager, UrlQueueManager>()
                            .AddSingleton<IUrlRepository, FileUrlRepository>()
                            .AddScoped<IPageStorage, PageStorage>()
                            .AddScoped<WebScraper>()
                            .AddDbContextFactory<AppDbContext>(options => options.UseSqlite(configuration.GetConnectionString("FileStorageDb"), b => b.MigrationsAssembly("DimonSmart.WebScraper")), ServiceLifetime.Transient)
                            .Configure<WebScraperSettings>(configuration.GetSection("WebScraperSettings"))
                            .Configure<StorageSettings>(configuration.GetSection("StorageSettings"));
                        services.AddSingleton(Log.Logger);
                    })
                    .Build();

                using (var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
                    dbContext.Database.Migrate();
                }

                await host.RunAsync();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
