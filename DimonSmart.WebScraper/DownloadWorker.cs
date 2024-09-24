using Serilog;
using System.Collections.Concurrent;

namespace DimonSmart.WebScraper;

public class DownloadWorker
{
    private readonly BlockingCollection<DownloadRequest> _requestQueue;
    private readonly ConcurrentBag<ScrapeResult> _results;
    private readonly IPageDownloader _pageDownloader;
    private readonly ILinkExtractor _pageHandler;
    private readonly IPageStorage _pageStorage;
    private readonly ILogger _logger;
    private readonly IUrlQueueManager _urlQueueManager;
    private readonly Action _onDownloadCompleted;

    public DownloadWorker(
        BlockingCollection<DownloadRequest> requestQueue,
        ConcurrentBag<ScrapeResult> results,
        IPageDownloader pageDownloader,
        ILinkExtractor pageHandler,
        IPageStorage pageStorage,
        ILogger logger,
        IUrlQueueManager urlQueueManager,
        Action onDownloadCompleted)
    {
        _requestQueue = requestQueue;
        _results = results;
        _pageDownloader = pageDownloader;
        _pageHandler = pageHandler;
        _pageStorage = pageStorage;
        _logger = logger;
        _urlQueueManager = urlQueueManager;
        _onDownloadCompleted = onDownloadCompleted;
    }

    public ScraperWorkerStatus WorkerStatus { get; } = new();

    public async Task RunAsync()
    {
        foreach (var request in _requestQueue.GetConsumingEnumerable())
        {
            WorkerStatus.SetWorkingStatus(request.Url);
            try
            {
                _logger.Information("Processing URL: {Url}", request.Url);
                await DoDownload(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing URL: {Url}", request.Url);
            }
            WorkerStatus.SetWaitStatus();
            _onDownloadCompleted();
        }
    }

    private async Task DoDownload(DownloadRequest request)
    {
        var pageContent = await _pageDownloader.DownloadPageContentAsync(request.Url);
        if (pageContent == null) return;

        long pageSize = pageContent.Length;
        _results.Add(new ScrapeResult { Url = request.Url, PageContent = pageContent });
        WorkerStatus.AddProcessedData(pageSize);

        var scrapedPage = new ScrapedWebPage(request.Url, pageContent);
        await _pageStorage.SavePageAsync(scrapedPage);

        if (request.Level > 0)
        {
            var links = _pageHandler.ExtractLinksFromPage(pageContent, request.Url);

            foreach (var link in links.Distinct())
            {
                if (_urlQueueManager.CanAddUrl(link))
                {
                    _requestQueue.Add(new DownloadRequest(link, request.Level - 1));
                }
            }
        }
    }
}
