using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DimonSmart.WebScraper;

public class UrlQueueManager(IUrlRepository repository, ILogger<UrlQueueManager> logger) : IUrlQueueManager
{
    private readonly HashSet<string> _addedUrls = new();

    private static readonly HashSet<string> LanguageCodes = new()
    {
        "af", "ar", "be", "bg", "ca", "cs", "da", "de", "el", "et", "fa", "fi", "fr", "he",
        "hi", "hr", "hu", "id", "it", "ja", "ko", "lt", "lv", "ms", "nl", "no", "pl", "pt",
        "ro", "ru", "sk", "sl", "sq", "sr", "sv", "th", "tr", "uk", "vi", "zh"
    };

    public bool CanAddUrl(string url)
    {
        if (!IsValidWebPageUrl(url))
        {
            logger.LogWarning("The URL '{Url}' is not a valid web page URL.", url);
            return false;
        }

        if (repository.ContainsProhibitedUrl(url))
        {
            logger.LogWarning("The URL '{Url}' is prohibited and cannot be added.", url);
            return false;
        }

        if (!LangFilter(url))
        {
            logger.LogWarning("The URL '{Url}' contains a prohibited language segment.", url);
            return false;
        }

        if (!_addedUrls.Add(url))
        {
            logger.LogInformation("The URL '{Url}' has already been added.", url);
            return false;
        }

        return true;
    }

    private static bool LangFilter(string url)
    {
        return !new Uri(url).Segments
            .Select(segment => segment.Trim('/'))
            .Any(segment => LanguageCodes.Contains(segment));
    }

    private static bool IsValidWebPageUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}