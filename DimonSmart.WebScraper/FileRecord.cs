namespace DimonSmart.WebScraper
{
    public class FileRecord
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
        public required string FileType { get; set; }
        public string? Metadata { get; set; }
    }
}