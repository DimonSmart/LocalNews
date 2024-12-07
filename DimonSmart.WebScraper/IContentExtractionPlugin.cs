namespace DimonSmart.WebScraper
{
    using HtmlAgilityPack;

    namespace DimonSmart.WebScraper
    {
        public interface IContentExtractionPlugin
        {
            void Process(HtmlDocument document);
        }
    }
}