namespace DimonSmart.WebScraper
{
    using HtmlAgilityPack;

    namespace DimonSmart.WebScraper
    {
        public class ContentExtractor : IContentExtractor
        {
            private readonly ContentExtractorOptions _options;

            public ContentExtractor(ContentExtractorOptions? options = null)
            {
                _options = options ?? new ContentExtractorOptions();
            }

            public string ExtractMainContent(string html)
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);

                foreach (var className in _options.ClassesToRemove)
                {
                    var nodesToRemove = document.DocumentNode.SelectNodes($"//*[contains(@class, '{className}')]");
                    if (nodesToRemove != null)
                    {
                        foreach (var node in nodesToRemove)
                        {
                            node.Remove();
                        }
                    }
                }

                foreach (var id in _options.IdsToRemove)
                {
                    var nodesToRemove = document.DocumentNode.SelectNodes($"//*[@id='{id}']");
                    if (nodesToRemove != null)
                    {
                        foreach (var node in nodesToRemove)
                        {
                            node.Remove();
                        }
                    }
                }

                foreach (var plugin in _options.Plugins)
                {
                    plugin.Process(document);
                }

                var text = document.DocumentNode.InnerText;

                if (_options.RemoveExtraSpaces)
                {
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", @" ").Trim();
                }

                return text;
            }
        }
    }
}