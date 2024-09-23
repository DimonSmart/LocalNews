namespace DimonSmart.WebScraper;

public interface IPageDownloader
{
    Task<string?> DownloadPageContentAsync(string url);
}