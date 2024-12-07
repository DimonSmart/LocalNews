using HtmlAgilityPack;
using DimonSmart.WebScraper.DimonSmart.WebScraper;

namespace DimonSmart.WebScraper.Tests
{
    public class ContentExtractorTests
    {
        private const string TestHtml = @"<html>
            <head><title>Test Page</title></head>
            <body>
                <header id='header'>Header Content</header>
                <nav class='nav'>Navigation Content</nav>
                <main>
                    <div id='main-content'>Main Content with <strong>important</strong> information.</div>
                </main>
                <footer class='footer'>Footer Content</footer>
            </body>
        </html>";

        [Fact]
        public void ExtractMainContent_RemovesFooterAndHeader()
        {
            var options = new ContentExtractorOptions();
            var extractor = new ContentExtractor(options);

            var result = extractor.ExtractMainContent(TestHtml);

            Assert.DoesNotContain("Header Content", result);
            Assert.DoesNotContain("Footer Content", result);
            Assert.Contains("Main Content with important information.", result);
        }

        [Fact]
        public void ExtractMainContent_WithPlugin_ProcessesDocument()
        {
            var options = new ContentExtractorOptions
            {
                Plugins = new List<IContentExtractionPlugin>
                {
                    new HeuristicContentExtractionPlugin()
                }
            };

            var extractor = new ContentExtractor(options);

            var result = extractor.ExtractMainContent(TestHtml);

            Assert.Equal("Main Content with important information.", result);
        }

        [Fact]
        public void HeuristicContentExtractionPlugin_ExtractsLargestTextNode()
        {
            var document = new HtmlDocument();
            document.LoadHtml(TestHtml);

            var plugin = new HeuristicContentExtractionPlugin();
            plugin.Process(document);

            var result = document.DocumentNode.InnerText.Trim();

            Assert.Equal("Main Content with important information.", result);
        }
    }
}
