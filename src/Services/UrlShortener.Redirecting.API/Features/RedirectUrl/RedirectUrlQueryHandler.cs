namespace UrlShortener.Redirecting.API.Features.RedirectUrl;

public record RedirectUrlQuery(string ShortUrl) : IQuery<RedirectUrlResult>;

public record RedirectUrlResult(string OriginalUrl);

public class RedirectUrlQueryHandler(IMongoCollection<ShortenedUrl> collection, IDistributedCache cache)
    : IQueryHandler<RedirectUrlQuery, RedirectUrlResult>
{
    public async Task<Result<RedirectUrlResult>> Handle(RedirectUrlQuery query, CancellationToken cancellationToken)
    {
        var filter = Builders<ShortenedUrl>.Filter.Eq(x => x.ShortUrl, query.ShortUrl);
        var updateDefinition = Builders<ShortenedUrl>.Update.Inc(x => x.TimesVisited, 1);

        var cachedRedirectUrl = await GetCachedRedirectUrl(query.ShortUrl, cancellationToken);

        if (cachedRedirectUrl != null)
        {
            await collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
            return new RedirectUrlResult(cachedRedirectUrl);
        }

        var shortenUrl = await
            collection.FindOneAndUpdateAsync(filter, updateDefinition, cancellationToken: cancellationToken);

        if (shortenUrl?.OriginalUrl is null) return Errors.OriginalUrlNotFound;

        if (shortenUrl.TimesVisited > 10)
            await cache.SetStringAsync(shortenUrl.ShortUrl, shortenUrl.OriginalUrl, cancellationToken);

        return new RedirectUrlResult(shortenUrl.OriginalUrl);
    }

    private async Task<string?> GetCachedRedirectUrl(string shortUrl, CancellationToken cancellationToken)
    {
        var longUrl = await cache.GetStringAsync(shortUrl, token: cancellationToken);

        return longUrl;
    }
}