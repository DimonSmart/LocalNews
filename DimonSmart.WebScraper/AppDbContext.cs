using Microsoft.EntityFrameworkCore;


namespace DimonSmart.WebScraper
{
    public class AppDbContext : DbContext
    {
        public DbSet<FileRecord> Files { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileRecord>().HasKey(f => f.Id);
        }
    }
}