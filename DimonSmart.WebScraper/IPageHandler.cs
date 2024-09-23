namespace DimonSmart.WebScraper;

public interface IPageHandler
{
    IEnumerable<string> ExtractLinksFromPage(string pageContent, string baseUrl);
}