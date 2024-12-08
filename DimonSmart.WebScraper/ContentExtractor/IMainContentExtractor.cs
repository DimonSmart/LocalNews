namespace DimonSmart.WebScraper
{

    public interface IMainContentExtractor
    {
        MainContent? ExtractMainContent(string uti, string html);
    }
}