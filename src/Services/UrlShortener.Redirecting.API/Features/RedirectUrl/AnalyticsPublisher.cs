namespace UrlShortener.Redirecting.API.Features.RedirectUrl;

public interface IAnalyticsPublisher
{
    Task PublishAsync(string shortCode);
}

public class AnalyticsPublisher(IConnectionMultiplexer redis) : IAnalyticsPublisher
{
    public async Task PublishAsync(string shortCode)
    {
        var db = redis.GetDatabase();
        await db.StringIncrementAsync($"count:{shortCode}");
    }
}