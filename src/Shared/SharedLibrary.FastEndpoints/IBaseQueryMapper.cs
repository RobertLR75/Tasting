using FastEndpoints;

namespace SharedLibrary.FastEndpoints;

public interface IBaseQueryMapper<in TRequest, TResponse, TQuery, TEntity> :
    IRequestMapper<TRequest, TEntity>,
    IResponseMapper<TResponse, TEntity>,
    IServiceResolverBase
    where TRequest : notnull
    where TResponse : notnull
{
    TQuery ToQuery(TRequest req);
    Task<TQuery> ToQueryAsync(TRequest req, CancellationToken ct);
}