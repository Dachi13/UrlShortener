using Microsoft.EntityFrameworkCore;
using UrlShortener.Shared.DbContext;

namespace UrlShortener.Analytics.Worker;

public interface IAnalyticsRepository
{
    Task UpdateCountsAsync(IEnumerable<(string ShortCode, long Count)> updates);
}

public class AnalyticsRepository(UrlShortenerDbContext context) : IAnalyticsRepository
{
    public async Task UpdateCountsAsync(IEnumerable<(string ShortCode, long Count)> updates)
    {
        foreach (var (shortCode, count) in updates)
        {
            var entity = await context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortUrl == shortCode);

            if (entity is not null)
            {
                entity.TimesVisited += count;
                // entity.LastVisitedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }
}