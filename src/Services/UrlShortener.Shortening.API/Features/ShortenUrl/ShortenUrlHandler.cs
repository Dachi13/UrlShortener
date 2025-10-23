namespace UrlShortener.Shortening.API.Features.ShortenUrl;

public record ShortenUrlCommand(string Url) : ICommand<ShortenUrlResult>;

public record ShortenUrlResult(string ShortenedUrl);

public class ShortenUrlHandler(IMongoCollection<ShortenedUrl> collection)
    : ICommandHandler<ShortenUrlCommand, ShortenUrlResult>
{
    public async Task<Result<ShortenUrlResult>> Handle(ShortenUrlCommand request, CancellationToken cancellationToken)
    {
        var shortenedUrl = GenerateShortenUri();

        var filter = Builders<ShortenedUrl>.Filter.Eq(x => x.ShortUrl, shortenedUrl);

        while (await collection.Find(filter).FirstOrDefaultAsync(cancellationToken) is not null)
            shortenedUrl = GenerateShortenUri();

        await collection.InsertOneAsync(new ShortenedUrl
        {
            OriginalUrl = request.Url,
            ShortUrl = shortenedUrl,
            TimesVisited = 0
        }, cancellationToken: cancellationToken);

        return new ShortenUrlResult(shortenedUrl);
    }

    private string GenerateShortenUri()
    {
        var randomChars = Guid.NewGuid().ToString().Substring(0, 7);

        return randomChars;
    }
}