namespace UrlShortener.Shortening.API.Features.ShortenUrl;

public record ShortenUrlCommand(string Url) : ICommand<ShortenUrlResult>;

public record ShortenUrlResult(string ShortenedUrl);

public class ShortenUrlHandler : ICommandHandler<ShortenUrlCommand, ShortenUrlResult>
{
    public Task<Result<ShortenUrlResult>> Handle(ShortenUrlCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}