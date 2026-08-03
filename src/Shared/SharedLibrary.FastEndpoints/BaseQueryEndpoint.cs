using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.FastEndpoints;

public abstract class BaseQueryEndpoint<TRequest, TResponse, TQuery, TEntity, TMapper>(IRequestHandler<TQuery, TEntity> handler) : Endpoint<TRequest, TResponse, TMapper>
    where TMapper : class, IBaseQueryMapper<TRequest, TResponse, TQuery, TEntity>
    where TQuery : class, IRequest<TEntity>
    where TEntity : class
    where TRequest : class
    where TResponse : class

{
    protected new IBaseQueryMapper<TRequest, TResponse, TQuery, TEntity> Map => base.Map;

    
    protected virtual TQuery ToQuery(TRequest req)
    {
        return Map.ToQuery(req);
    }
    
    protected virtual async Task<TEntity> HandleQueryAsync(TQuery query, CancellationToken ct = default)
    {
        return await handler.HandleAsync(query, ct);
    }

    protected virtual async Task HandleEntityAsync(TEntity entity, CancellationToken ct = default)
    {
        
    }
    protected virtual async Task HandleResponseAsync(TResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
    
    public override async Task HandleAsync(TRequest req, CancellationToken ct)
    {
        var query = ToQuery(req);
        var entity = await HandleQueryAsync(query, ct);
        await HandleEntityAsync(entity, ct);
        Response = await Map.FromEntityAsync(entity, ct);
        await HandleResponseAsync(Response, ct);
    }
}