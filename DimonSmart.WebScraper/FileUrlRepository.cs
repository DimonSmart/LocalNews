using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class FileUrlRepository : IUrlRepository
{
    private readonly HashSet<string> _prohibitedDomains = [];
    private readonly HashSet<string> _prohibitedUrls = [];
    private readonly WebScraperSettings _settings;

    public FileUrlRepository(IOptions<WebScraperSettings> settings)
    {
        _settings = settings.Value;

        if (!File.Exists(_settings.ProhibitedUrlsFileName))
            return;

        foreach (var line in File.ReadAllLines(_settings.ProhibitedUrlsFileName))
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var lowerLine = trimmedLine.ToLowerInvariant();

            if (Uri.TryCreate(lowerLine, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                _prohibitedUrls.Add(lowerLine);
            }
            else
            {
                _prohibitedDomains.Add(lowerLine);
            }
        }
    }

    public bool ContainsProhibitedUrl(string url)
    {
        var lowerUrl = url.ToLowerInvariant();

        if (_prohibitedUrls.Contains(lowerUrl))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return false;
        }

        var host = uriResult.Host.ToLowerInvariant();

        return _prohibitedDomains.Any(domain =>
            host == domain || host.EndsWith("." + domain));

    }
}