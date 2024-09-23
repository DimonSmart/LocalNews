namespace DimonSmart.WebScraper;

public interface IPageStorage
{
    Task SavePageAsync(ScrapedWebPage page);
}