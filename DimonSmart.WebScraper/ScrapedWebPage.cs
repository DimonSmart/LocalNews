namespace DimonSmart.WebScraper;

public record ScrapedWebPage(string Url, string HtmlContent, MainContent MainContent)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime DownloadedAt { get; init; } = DateTime.UtcNow;
}
