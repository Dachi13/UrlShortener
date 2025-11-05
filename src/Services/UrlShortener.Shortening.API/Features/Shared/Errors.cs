namespace UrlShortener.Shortening.API.Features.Shared;

public static class Errors
{
    public static Error HashLengthError =
        new("HASH_LENGTH_ERROR", "Could not hashing length or the number was invalid", ErrorType.ConfigurationError);
}