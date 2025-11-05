namespace UrlShortener.Redirecting.API.Features.RedirectUrl;

public record RedirectUrlQuery(string ShortUrl) : IQuery<RedirectUrlResult>;

public record RedirectUrlResult(string OriginalUrl);

public class RedirectUrlQueryHandler(
    IMongoCollection<ShortenedUrl> collection,
    IConnectionMultiplexer redis,
    IAnalyticsPublisher analyticsPublisher)
    : IQueryHandler<RedirectUrlQuery, RedirectUrlResult>
{
    private IDatabase Cache => redis.GetDatabase();

    public async Task<Result<RedirectUrlResult>> Handle(RedirectUrlQuery query, CancellationToken cancellationToken)
    {
        var filter = Builders<ShortenedUrl>.Filter.Eq(x => x.ShortUrl, query.ShortUrl);
        var updateDefinition = Builders<ShortenedUrl>.Update.Inc(x => x.TimesVisited, 1);

        var cachedRedirectUrl = await Cache.StringGetAsync(query.ShortUrl);

        if (cachedRedirectUrl is { IsNull: false, HasValue: true })
        {
            await collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
            _ = analyticsPublisher.PublishAsync(cachedRedirectUrl.ToString());
            return new RedirectUrlResult(cachedRedirectUrl.ToString());
        }

        var shortenUrl = await
            collection.FindOneAndUpdateAsync(filter, updateDefinition, cancellationToken: cancellationToken);

        if (shortenUrl?.OriginalUrl is null) return Errors.OriginalUrlNotFound;

        if (shortenUrl.TimesVisited > 10)
            await Cache.StringSetAsync(shortenUrl.ShortUrl, shortenUrl.OriginalUrl);

        return new RedirectUrlResult(shortenUrl.OriginalUrl);
    }
}