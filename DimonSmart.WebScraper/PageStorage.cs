using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class PageStorage : IPageStorage
{
    private readonly string _storagePath;

    public PageStorage(IOptions<StorageSettings> settings)
    {
        _storagePath = settings.Value.StoragePath;
    }

    public async Task SavePageAsync(ScrapedWebPage page)
    {
        var filePath = Path.Combine(_storagePath, $"{page.Id}.html");
        await File.WriteAllTextAsync(filePath, page.Content);
    }
}
