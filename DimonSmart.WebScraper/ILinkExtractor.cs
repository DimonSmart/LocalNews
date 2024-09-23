namespace DimonSmart.WebScraper;

public interface ILinkExtractor
{
    IEnumerable<string> ExtractLinksFromPage(string pageContent, string baseUrl);
}