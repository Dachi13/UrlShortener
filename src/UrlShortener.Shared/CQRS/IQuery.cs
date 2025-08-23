namespace UrlShortener.Shared.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> where TResponse : notnull;