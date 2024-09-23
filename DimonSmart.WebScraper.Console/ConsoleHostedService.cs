using Microsoft.Extensions.Hosting;

namespace DimonSmart.WebScraper.Console;

public class ConsoleHostedService(WebScraper webScraper) : IHostedService
{
    private readonly WebScraper _webScraper = webScraper;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var downloadRequests = new List<DownloadRequest>
        {
            new DownloadRequest("https://visita.malaga.eu/en/", 2)
        };

        await _webScraper.ScrapAsync(downloadRequests);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}