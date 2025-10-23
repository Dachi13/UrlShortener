namespace UrlShortener.Redirecting.API.Features.Shared;

public static class Errors
{
    public static Error OriginalUrlNotFound = new("URL_NOT_FOUND", "Original url was not found", ErrorType.NotFound);
}