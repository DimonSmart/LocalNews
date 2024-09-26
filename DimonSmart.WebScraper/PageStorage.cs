using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class PageStorage : IPageStorage
{
    private readonly string _storagePath;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public PageStorage(IOptions<StorageSettings> settings, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _storagePath = settings.Value.StoragePath;
        _dbContextFactory = dbContextFactory;
    }

    public async Task SavePageAsync(ScrapedWebPage page)
    {
        var directory1 = page.Id.ToString()[0].ToString();
        var directory2 = page.Id.ToString()[1].ToString();
        var folderPath = Path.Combine(_storagePath, directory1, directory2);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{page.Id}.html";
        var filePath = Path.Combine(folderPath, fileName);
        await File.WriteAllTextAsync(filePath, page.Content);

        var fileRecord = new FileRecord
        {
            Id = page.Id,
            FileName = fileName,
            CreatedAt = DateTime.UtcNow,
            Size = page.SizeInBytes,
            FileType = "html",
            Metadata = null
        };

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.Files.Add(fileRecord);
        await dbContext.SaveChangesAsync();
    }
}
