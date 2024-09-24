namespace DimonSmart.WebScraper;

public interface IUrlRepository
{
    bool ContainsProhibitedUrl(string url);
}