namespace UrlShortener.Shortening.API;

public class UrlShortenerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    public static UrlShortenerDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ShortenedUrl>().ToCollection("shortened_urls");
    }
}