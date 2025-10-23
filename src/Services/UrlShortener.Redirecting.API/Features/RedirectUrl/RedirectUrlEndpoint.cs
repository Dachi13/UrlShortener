namespace UrlShortener.Redirecting.API.Features.RedirectUrl;

public record RedirectUrlResponse(string RedirectUrl);

public static class RedirectUrlEndpoint
{
    public static void AddRedirectUrlRoute(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{shortenedUrl}", async (string shortenedUrl, ISender sender) =>
            {
                var storeBasketCommand = new RedirectUrlQuery(shortenedUrl);

                var result = await sender.Send(storeBasketCommand);

                return result.Match(
                    _ => Results.Redirect(result.Value.OriginalUrl),
                    error => error switch
                    {
                        { ErrorType: ErrorType.InternalServerError } => Results.Conflict(
                            new { message = error.Message }),
                        {ErrorType: ErrorType.NotFound } => Results.NotFound(error.Message),
                        _ => Results.Conflict(error.Message)
                    });
            })
            .WithName("Url Redirector")
            .Produces<RedirectUrlResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Redirects to the original url")
            .WithDescription("Redirects to the original url");
    }
}