namespace UrlShortener.Shortening.API.Features.ShortenUrl;

public record ShortenUrl(string Url);

public record ShortenUrlResponse(string ShortenedUrl);

public static class ShortenUrlEndpoint
{
    public static void AddShortenRoute(this IEndpointRouteBuilder app)
    {
        app.MapPost("/ShortenUrl", async (ShortenUrl request, ISender sender) =>
            {
                var storeBasketCommand = new ShortenUrlCommand(request.Url);

                var result = await sender.Send(storeBasketCommand);

                return result.Match(
                    _ => Results.Created($"/shortenedUrls/{result.Value.ShortenedUrl}", result.Value),
                    error => error switch
                    {
                        { ErrorType: ErrorType.InternalServerError } => Results.Conflict(
                            new { message = error.Message }),
                        _ => Results.Conflict(error.Message)
                    });
            })
            .WithName("Url Shortener")
            .Produces<ShortenUrlResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Shortens Url")
            .WithDescription("Shortens Url");
    }
}