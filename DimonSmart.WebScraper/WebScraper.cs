using System.Collections.Concurrent;

namespace DimonSmart.WebScraper;

public class WebScraper
{
    private readonly BlockingCollection<DownloadRequest> _requestQueue = new();
    private readonly ConcurrentBag<ScrapeResult> _results = new();
    private readonly List<ScraperWorkerStatus> _workerStatuses = new();
    private int _activeThreads;
    private readonly int _maxThreads;
    private bool _isWorkComplete;

    private readonly IPageDownloader _pageDownloader;
    private readonly IPageHandler _pageHandler;
    private readonly IPageStorage _pageStorage;
    private readonly ILogger _logger;
    private readonly IUrlQueueManager _urlQueueManager;

    public WebScraper(int maxThreads, IPageDownloader pageDownloader, IPageHandler pageHandler, IPageStorage pageStorage, ILogger logger, IUrlQueueManager urlQueueManager)
    {
        _maxThreads = maxThreads;
        _pageDownloader = pageDownloader;
        _pageHandler = pageHandler;
        _pageStorage = pageStorage;
        _logger = logger;
        _urlQueueManager = urlQueueManager;
    }

    public async Task<IReadOnlyCollection<ScrapeResult>> ScrapAsync(IEnumerable<DownloadRequest> initialRequests)
    {
        var consumerTasks = StartConsumers();

        foreach (var request in initialRequests)
        {
            if (_urlQueueManager.CanAddUrl(request.Url))
            {
                _requestQueue.Add(request);
            }
        }

        await Task.WhenAll(consumerTasks);
        return _results.ToList();
    }

    private Task[] StartConsumers()
    {
        var consumerTasks = new Task[_maxThreads];
        for (var i = 0; i < _maxThreads; i++)
        {
            var workerIndex = i;
            _workerStatuses.Add(new ScraperWorkerStatus());
            consumerTasks[i] = Task.Run(() => ConsumeAsync(workerIndex));
        }
        return consumerTasks;
    }

    private async Task ConsumeAsync(int workerIndex)
    {
        var workerStatus = _workerStatuses[workerIndex];

        while (!_requestQueue.IsCompleted || _requestQueue.Count > 0)
        {
            DownloadRequest request;

            lock (this)
            {
                while (_requestQueue.Count == 0 && !_isWorkComplete)
                {
                    workerStatus.SetWaitStatus();
                    Monitor.Wait(this);
                }

                if (_isWorkComplete)
                    break;

                if (_requestQueue.TryTake(out request))
                {
                    Interlocked.Increment(ref _activeThreads);
                    workerStatus.SetWorkingStatus(request.Url);
                }
            }

            try
            {
                var pageContent = await _pageDownloader.DownloadPageContentAsync(request.Url);
                if (pageContent != null)
                {
                    long pageSize = pageContent.Length;
                    _results.Add(new ScrapeResult { Url = request.Url, PageContent = pageContent });
                    workerStatus.AddProcessedData(pageSize);

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
            catch (Exception ex)
            {
                _logger.LogError($"Error processing URL {request.Url}: {ex.Message}");
            }

            Interlocked.Decrement(ref _activeThreads);

            lock (this)
            {
                if (_requestQueue.Count != 0 || _activeThreads != 0) continue;
                _isWorkComplete = true;
                Monitor.PulseAll(this);
            }
        }
    }
}