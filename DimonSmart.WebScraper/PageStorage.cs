using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class PageStorage(IOptions<StorageSettings> settings, IDbContextFactory<AppDbContext> dbContextFactory) : IPageStorage
{
    private readonly string _storagePath = settings.Value.StoragePath;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;

    public async Task SavePageAsync(ScrapedWebPage page)
    {
        var directory1 = page.Id.ToString()[0].ToString();
        var directory2 = page.Id.ToString()[1].ToString();
        var folderPath = Path.Combine(_storagePath, directory1, directory2);

        Directory.CreateDirectory(folderPath);
        await File.WriteAllTextAsync(Path.Combine(folderPath, $"{page.Id}.html"), page.HtmlContent);
        await File.WriteAllTextAsync(Path.Combine(folderPath, $"{page.Id}.txt"), page.MainContent.Content);

        var fileRecord = new FileRecord
        {
            Id = page.Id,
            CreatedAt = page.DownloadedAt,
            MainContentSize = page.MainContent.Content.Length,
            HTMLContentSize = page.HtmlContent.Length,
            Title = page.MainContent.Title
        };

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Files.Add(fileRecord);
        await dbContext.SaveChangesAsync();
    }
}
