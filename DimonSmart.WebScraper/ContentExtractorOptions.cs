namespace DimonSmart.WebScraper
{
    namespace DimonSmart.WebScraper
    {
        public class ContentExtractorOptions
        {
            public List<string> ClassesToRemove { get; set; } = new List<string> { "footer", "nav", "header" };
            public List<string> IdsToRemove { get; set; } = new List<string> { "header", "footer" };
            public bool RemoveExtraSpaces { get; set; } = true;
            public List<IContentExtractionPlugin> Plugins { get; set; } = new List<IContentExtractionPlugin>();
        }
    }
}