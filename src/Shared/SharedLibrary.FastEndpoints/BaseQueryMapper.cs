using FastEndpoints;

namespace SharedLibrary.FastEndpoints;

public abstract class BaseQueryMapper<TRequest, TResponse, TQuery, TEntity> : Mapper<TRequest, TResponse, TEntity>, IBaseQueryMapper<TRequest, TResponse, TQuery, TEntity> 
    where TRequest : notnull where TResponse : notnull
{
    public abstract TQuery ToQuery(TRequest req);

    public virtual Task<TQuery> ToQueryAsync(TRequest req, CancellationToken ct)
        => Task.FromResult(ToQuery(req));
}