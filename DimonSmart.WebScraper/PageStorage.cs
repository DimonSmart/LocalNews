using Microsoft.Extensions.Options;

namespace DimonSmart.WebScraper;

public class PageStorage(IOptions<StorageSettings> settings, AppDbContext dbContext) : IPageStorage
{
    private readonly string _storagePath = settings.Value.StoragePath;

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

        dbContext.Files.Add(fileRecord);
        await dbContext.SaveChangesAsync();
    }
}