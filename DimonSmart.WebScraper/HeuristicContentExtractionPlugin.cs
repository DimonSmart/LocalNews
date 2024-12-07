using DimonSmart.WebScraper.DimonSmart.WebScraper;
using HtmlAgilityPack;

namespace DimonSmart.WebScraper
{
    public class HeuristicContentExtractionPlugin : IContentExtractionPlugin
    {
        public void Process(HtmlDocument document)
        {
            var bodyNode = document.DocumentNode.SelectSingleNode("//body");
            if (bodyNode == null) return;

            HtmlNode? bestNode = null;
            var maxTextLength = 0;

            // Evaluate all descendant nodes of <body>
            foreach (var node in bodyNode.Descendants().Where(n => n.NodeType == HtmlNodeType.Element))
            {
                if (node.Name == "script" || node.Name == "style")
                    continue;

                // Calculate text length of the current node
                var text = node.InnerText.Trim();
                var textLength = text.Length;

                // Prioritize nodes with the most text
                if (textLength > maxTextLength && !string.IsNullOrWhiteSpace(text))
                {
                    maxTextLength = textLength;
                    bestNode = node;
                }
            }

            if (bestNode != null)
            {
                document.DocumentNode.RemoveAllChildren();
                document.DocumentNode.AppendChild(bestNode.Clone());
            }
        }
    }
}
