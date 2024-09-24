using Microsoft.Extensions.Logging;

namespace DimonSmart.WebScraper;

public class UrlQueueManager(IUrlRepository repository, ILogger<UrlQueueManager> logger) : IUrlQueueManager
{
    private static readonly HashSet<string> LanguageCodes =
    [
        "af", "ar", "be", "bg", "ca", "cs", "da", "de", "el", "et", "fa", "fi", "fr", "he",
        "hi", "hr", "hu", "id", "it", "ja", "ko", "lt", "lv", "ms", "nl", "no", "pl", "pt",
        "ro", "ru", "sk", "sl", "sq", "sr", "sv", "th", "tr", "uk", "vi", "zh"
    ];

    private readonly HashSet<string> _addedUrls = new();

    public bool CanAddUrl(string url)
    {
        var urlWithoutFragment = RemoveFragment(url);

        if (!IsValidWebPageUrl(urlWithoutFragment))
        {
            logger.LogTrace("The URL '{Url}' is not a valid web page URL.", url);
            return false;
        }

        if (repository.ContainsProhibitedUrl(urlWithoutFragment))
        {
            logger.LogTrace("The URL '{Url}' is prohibited and cannot be added.", url);
            return false;
        }

        if (!LangFilter(urlWithoutFragment))
        {
            logger.LogTrace("The URL '{Url}' contains a prohibited language segment.", url);
            return false;
        }

        if (!_addedUrls.Add(urlWithoutFragment))
        {
            logger.LogTrace("The URL '{Url}' has already been added.", url);
            return false;
        }

        return true;
    }

    private static bool LangFilter(string url)
    {
        var uri = new Uri(url);
        return !uri.Segments
            .Select(segment => segment.Trim('/'))
            .Any(segment => LanguageCodes.Contains(segment));
    }

    private static bool IsValidWebPageUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private static string RemoveFragment(string url)
    {
        var uri = new Uri(url);
        var uriWithoutFragment = new UriBuilder(uri)
        {
            Fragment = string.Empty
        }.Uri;
        return uriWithoutFragment.ToString();
    }
}