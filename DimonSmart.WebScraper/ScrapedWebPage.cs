namespace DimonSmart.WebScraper;

public class ScrapedWebPage(string url, string content)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Url { get; set; } = url;
    public string Content { get; set; } = content;
    public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    public long SizeInBytes => Content?.Length ?? 0;
}
