namespace DimonSmart.WebScraper;

public class PageStorage : IPageStorage
{
    // Hardcoded path, later to be passed from config

    private const string StoragePath = @"E:\DownloadedPages\";

    public async Task SavePageAsync(ScrapedWebPage page)
    {
        var filePath = Path.Combine(StoragePath, $"{page.Id}.html");
        await File.WriteAllTextAsync(filePath, page.Content);
    }
}