using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DimonSmart.WebScraper
{
    public class WebScraper
    {
        private readonly BlockingCollection<DownloadRequest> _requestQueue = new();
        private readonly ConcurrentBag<ScrapeResult> _results = new();
        private readonly List<DownloadWorker> _workers = new();
        private readonly int _maxThreads;

        private readonly IPageDownloader _pageDownloader;
        private readonly IPageHandler _pageHandler;
        private readonly IPageStorage _pageStorage;
        private readonly ILogger _logger;
        private readonly IUrlQueueManager _urlQueueManager;

        private int _pendingTasks;

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
            Interlocked.Increment(ref _pendingTasks);
        }

        private Task[] StartConsumers()
        {
            var consumerTasks = new Task[_maxThreads];

            for (var i = 0; i < _maxThreads; i++)
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
            if (Interlocked.Decrement(ref _pendingTasks) == 0)
            {
                // All tasks are completed, complete the queue
                _requestQueue.CompleteAdding();
            }
        }
    }
}
