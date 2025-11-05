namespace UrlShortener.Shortening.API.Features.ShortenUrl;

public record ShortenUrlCommand(string Url) : ICommand<ShortenUrlResult>;

public record ShortenUrlResult(string ShortenedUrl);

public class ShortenUrlHandler(
    IMongoCollection<ShortenedUrl> collection,
    ConfigurationManager configuration,
    ILogger<ShortenUrlHandler> logger)
    : ICommandHandler<ShortenUrlCommand, ShortenUrlResult>
{
    public async Task<Result<ShortenUrlResult>> Handle(ShortenUrlCommand request, CancellationToken cancellationToken)
    {
        var hashedUrlResult = GenerateShortenUri();

        if (!hashedUrlResult.IsSuccess)
        {
            logger.LogError("Error occured while generating shorten url. Exception message: {Error}",
                hashedUrlResult.Error.Message);
            return hashedUrlResult.Error;
        }

        var hashedUrl = hashedUrlResult.Value;

        var filter = Builders<ShortenedUrl>.Filter.Eq(x => x.ShortUrl, hashedUrl);

        while (await collection.Find(filter).FirstOrDefaultAsync(cancellationToken) is not null)
        {
            hashedUrlResult = GenerateShortenUri();

            if (!hashedUrlResult.IsSuccess)
            {
                logger.LogError("Error occured while generating shorten url. Exception message: {Error}",
                    hashedUrlResult.Error.Message);
                return hashedUrlResult.Error;
            }

            // TODO add redis cache for counting collisions
            logger.LogWarning("Hashed Url collision. Suggest to increase shorten url length.");

            hashedUrl = hashedUrlResult.Value;
        }

        await collection.InsertOneAsync(new ShortenedUrl
        {
            OriginalUrl = request.Url,
            ShortUrl = hashedUrl,
            TimesVisited = 0
        }, cancellationToken: cancellationToken);

        return new ShortenUrlResult(hashedUrl);
    }

    private Result<string> GenerateShortenUri()
    {
        var wasParsed = int.TryParse(configuration["HashingLength"], out var hashLength);

        if (!wasParsed || hashLength == 0) return Errors.HashLengthError;

        var randomChars = Guid.NewGuid().ToString().Substring(0, hashLength);

        return randomChars;
    }
}