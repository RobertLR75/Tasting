using FastEndpoints;

namespace SharedLibrary.FastEndpoints;

public abstract class BaseCommandMapper<TRequest, TResponse, TCommand, TEntity> : Mapper<TRequest, TResponse, TEntity>, IBaseCommandMapper<TRequest, TResponse, TCommand, TEntity> 
    where TRequest : notnull where TResponse : notnull
{
    public abstract TCommand ToCommand(TRequest req);

    public virtual Task<TCommand> ToCommandAsync(TRequest req, CancellationToken ct)
        => Task.FromResult(ToCommand(req));
}