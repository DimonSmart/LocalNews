using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class FileUrlRepository : IUrlRepository
{
    private readonly WebScraperSettings _settings;
    private readonly HashSet<string> _urls = [];

    public FileUrlRepository(IOptions<WebScraperSettings> settings)
    {
        _settings = settings.Value;
        if (!File.Exists(_settings.ProhibitedUrlsFileName)) return;

        foreach (var line in File.ReadAllLines(_settings.ProhibitedUrlsFileName))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            _urls.Add(line);
        }
    }

    public bool ContainsProhibitedUrl(string url)
    {
        return _urls.Contains(url);
    }

    public void Add(string url)
    {
        _urls.Add(url);
    }
}
