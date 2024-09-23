using System.Collections.Concurrent;

namespace DimonSmart.WebScraper;

public class DownloadWorker(
    BlockingCollection<DownloadRequest> requestQueue,
    ConcurrentBag<ScrapeResult> results,
    IPageDownloader pageDownloader,
    IPageHandler pageHandler,
    IPageStorage pageStorage,
    ILogger logger,
    IUrlQueueManager urlQueueManager,
    Action onDownloadCompleted)
{
    public ScraperWorkerStatus WorkerStatus { get; } = new();

    public async Task RunAsync()
    {
        foreach (var request in requestQueue.GetConsumingEnumerable())
        {
            WorkerStatus.SetWorkingStatus(request.Url);
            await DoDownload(request);
            WorkerStatus.SetWaitStatus();
        }
    }

    private async Task DoDownload(DownloadRequest request)
    {
        var pageContent = await pageDownloader.DownloadPageContentAsync(request.Url);
        if (pageContent == null) return;

        long pageSize = pageContent.Length;
        results.Add(new ScrapeResult { Url = request.Url, PageContent = pageContent });
        WorkerStatus.AddProcessedData(pageSize);

        var scrapedPage = new ScrapedWebPage(request.Url, pageContent);
        await pageStorage.SavePageAsync(scrapedPage);

        if (request.Level > 0)
        {
            var links = pageHandler.ExtractLinksFromPage(pageContent, request.Url);

            foreach (var link in links.Distinct())
            {
                if (urlQueueManager.CanAddUrl(link))
                {
                    requestQueue.Add(new DownloadRequest(link, request.Level - 1));
                }
            }
        }

        onDownloadCompleted();
    }
}