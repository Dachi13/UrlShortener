namespace UrlShortener.Shortening.API.Features.ShortenUrl;

public record ShortenUrlCommand(string Url) : ICommand<ShortenUrlResult>;

public record ShortenUrlResult(string ShortenedUrl);

public class ShortenUrlHandler(IMongoCollection<ShortenedUrl> collection, ConfigurationManager configuration)
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
            ShortUrl = shortenedUrl
        }, cancellationToken: cancellationToken);

        return new ShortenUrlResult(shortenedUrl);
    }

    private string GenerateShortenUri()
    {
        var random = new Random();
        var url = configuration["WebsiteUrl"]!;

        var shortenedUrl = new StringBuilder(url + "/");

        for (var i = 0; i < 7; i++)
        {
            var charToGenerate = random.Next(0, 2);

            var asciiNumber = charToGenerate switch
            {
                (int)RandomCharType.Number => random.Next(48, 58),
                (int)RandomCharType.LowerCaseCharacter => random.Next(97, 123),
                (int)RandomCharType.UpperCaseCharacter => random.Next(65, 91)
            };

            var character = (char)asciiNumber;

            shortenedUrl.Append(character);
        }

        return shortenedUrl.ToString();
    }
}

public enum RandomCharType
{
    Number,
    LowerCaseCharacter,
    UpperCaseCharacter
}