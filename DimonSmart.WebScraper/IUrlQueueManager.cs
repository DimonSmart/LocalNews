namespace DimonSmart.WebScraper;

public interface IUrlQueueManager
{
    bool CanAddUrl(string url);
}