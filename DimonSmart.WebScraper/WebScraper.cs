using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using Serilog;

namespace DimonSmart.WebScraper;

public class WebScraper
{
    private readonly ILogger _logger;
    private readonly IPageDownloader _pageDownloader;
    private readonly ILinkExtractor _pageHandler;
    private readonly IPageStorage _pageStorage;
    private readonly BlockingCollection<DownloadRequest> _requestQueue = new();
    private readonly ConcurrentBag<ScrapeResult> _results = [];
    private readonly IUrlQueueManager _urlQueueManager;
    private readonly List<DownloadWorker> _workers = [];
    private readonly WebScraperSettings _webScraperSettings;

    public WebScraper(IOptions<WebScraperSettings> settings, IPageDownloader pageDownloader, ILinkExtractor pageHandler,
        IPageStorage pageStorage, ILogger logger, IUrlQueueManager urlQueueManager)
    {
        _webScraperSettings = settings.Value;
        _pageDownloader = pageDownloader;
        _pageHandler = pageHandler;
        _pageStorage = pageStorage;
        _logger = logger;
        _urlQueueManager = urlQueueManager;
    }

    public async Task<IReadOnlyCollection<ScrapeResult>> ScrapAsync(IEnumerable<DownloadRequest> initialRequests)
    {
        foreach (var request in initialRequests)
        {
            if (_urlQueueManager.CanAddUrl(request.Url))
            {
                EnqueueRequest(request);
            }
        }

        var consumerTasks = StartConsumers();

        await Task.WhenAll(consumerTasks);

        return _results.ToList();
    }

    private void EnqueueRequest(DownloadRequest request)
    {
        _requestQueue.Add(request);
    }

    private Task[] StartConsumers()
    {
        var consumerTasks = new Task[_webScraperSettings.MaxThreads];

        for (var i = 0; i < _webScraperSettings.MaxThreads; i++)
        {
            var worker = new DownloadWorker(
                _requestQueue,
                _results,
                _pageDownloader,
                _pageHandler,
                _pageStorage,
                _logger,
                _urlQueueManager,
                CheckCompletion);

            _workers.Add(worker);

            consumerTasks[i] = Task.Run(() => worker.RunAsync());
        }

        return consumerTasks;
    }

    private void CheckCompletion()
    {
        if (_requestQueue.Count == 0 && _workers.All(w => w.WorkerStatus.Status == WebScraperWorkerStatus.Waiting))
            _requestQueue.CompleteAdding();
    }
}