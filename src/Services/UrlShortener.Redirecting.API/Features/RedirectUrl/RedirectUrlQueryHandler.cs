namespace UrlShortener.Redirecting.API.Features.RedirectUrl;

public record RedirectUrlQuery(string ShortUrl) : IQuery<RedirectUrlResult>;

public record RedirectUrlResult(string OriginalUrl);

public class RedirectUrlQueryHandler(IMongoCollection<ShortenedUrl> collection)
    : IQueryHandler<RedirectUrlQuery, RedirectUrlResult>
{
    public async Task<Result<RedirectUrlResult>> Handle(RedirectUrlQuery query, CancellationToken cancellationToken)
    {
        var filter = Builders<ShortenedUrl>.Filter.Eq(x => x.ShortUrl, query.ShortUrl);

        var shortenUrl = await
            collection.FindOneAndUpdateAsync(filter, Builders<ShortenedUrl>.Update.Inc(x => x.TimesVisited, 1),
                cancellationToken: cancellationToken);

        if (shortenUrl is null) return Errors.OriginalUrlNotFound;

        return new RedirectUrlResult(shortenUrl.OriginalUrl);
    }
}