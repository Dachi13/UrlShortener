namespace UrlShortener.Shortening.API.Models;

public class ShortenedUrl
{
    [BsonId] public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OriginalUrl { get; set; } = null!;
    public string ShortUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}