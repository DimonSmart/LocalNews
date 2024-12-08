using DimonSmart.WebScraper;
using SmartReader;
using System.Diagnostics;

public class MainContentExtractor : IMainContentExtractor
{
    public MainContent? ExtractMainContent(string uri, string html)
    {
        var article = new Reader(uri, html).GetArticle();
        return !string.IsNullOrWhiteSpace(article.TextContent) ? new MainContent(article.Title, article.TextContent) : null;
    }
}
