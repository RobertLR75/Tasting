using FastEndpoints;

namespace SharedLibrary.FastEndpoints;

public interface IBaseCommandMapper<in TRequest, TResponse, TCommand, TEntity> :
    IRequestMapper<TRequest, TEntity>,
    IResponseMapper<TResponse, TEntity>,
    IServiceResolverBase
    where TRequest : notnull
    where TResponse : notnull
{
    TCommand ToCommand(TRequest req);
    Task<TCommand> ToCommandAsync(TRequest req, CancellationToken ct);
}