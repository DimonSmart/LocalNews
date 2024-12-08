using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DimonSmart.WebScraper.Console;

public class ConsoleHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ConsoleHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var webScraper = scope.ServiceProvider.GetRequiredService<WebScraper>();

        var downloadRequests = new List<DownloadRequest>
        {
           new("https://visita.malaga.eu/en/", 2)
        };

        await webScraper.ScrapAsync(downloadRequests);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
