namespace UrlShortener.Shared.Models;

public enum ErrorType
{
    None,
    Conflict,
    NotFound,
    Validation,
    DatabaseError,
    InternalServerError,
    UnprocessableEntity,
    InvalidRequest,
    ConfigurationError
}