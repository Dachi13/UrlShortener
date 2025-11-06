namespace UrlShortener.Analytics.Worker;

public class Worker(
    ILogger<Worker> logger,
    IConnectionMultiplexer redis,
    IAnalyticsRepository analyticsRepo) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await FlushAnalyticsAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task FlushAnalyticsAsync()
    {
        var db = redis.GetDatabase();
        var server = GetServer();

        var keys = server.Keys(pattern: "count:*").ToArray();

        if (!keys.Any())
        {
            logger.LogInformation("No analytics keys found in Redis.");
            return;
        }

        var updates = new List<(string ShortCode, long Count)>();

        foreach (var key in keys)
        {
            var shortCode = key.ToString().Replace("count:", "");
            var count = (long)await db.StringGetAsync(key);
            updates.Add((shortCode, count));
        }

        await analyticsRepo.UpdateCountsAsync(updates);

        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }

        logger.LogInformation("Flushed {Count} analytics keys to database.", updates.Count);
    }

    private IServer GetServer()
    {
        var endpoint = redis.GetEndPoints().First();
        return redis.GetServer(endpoint);
    }
}