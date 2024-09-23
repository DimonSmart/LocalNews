using HtmlAgilityPack;

namespace DimonSmart.WebScraper;

public class LinkExtractor : IPageHandler
{
    public IEnumerable<string> ExtractLinksFromPage(string pageContent, string baseUrl)
    {
        var links = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        var anchorNodes = doc.DocumentNode.SelectNodes("//a[@href]");
        if (anchorNodes == null) return links;

        foreach (var node in anchorNodes)
        {
            var hrefValue = node.GetAttributeValue("href", string.Empty);
            if (!string.IsNullOrEmpty(hrefValue))
            {
                var absoluteUrl = GetAbsoluteUrl(hrefValue, baseUrl);
                if (absoluteUrl != null)
                {
                    links.Add(absoluteUrl);
                }
            }
        }

        return links;
    }

    private string? GetAbsoluteUrl(string url, string baseUrl)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return url;
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            return new Uri(baseUri, url).ToString();
        }

        return null;
    }
}