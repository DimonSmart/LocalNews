namespace DimonSmart.WebScraper
{
    public class FileRecord
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public long MainContentSize { get; set; }
        public long HTMLContentSize { get; set; }
        public string Title { get; set; }
    }
}