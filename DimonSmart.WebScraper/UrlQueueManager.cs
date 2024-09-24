namespace DimonSmart.WebScraper;

public class UrlQueueManager(IUrlRepository repository) : IUrlQueueManager
{
    private readonly HashSet<string> _addedUrls = [];

    public bool CanAddUrl(string url) => !repository.ContainsProhibitedUrl(url) && _addedUrls.Add(url);
}