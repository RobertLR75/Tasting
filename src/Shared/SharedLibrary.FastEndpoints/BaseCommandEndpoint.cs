using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.FastEndpoints;

public abstract class BaseCommandEndpoint<TRequest, TResponse, TCommand, TEntity, TMapper>(IRequestHandler<TCommand, TEntity> handler) : Endpoint<TRequest, TResponse, TMapper> 
    where TMapper : class, IBaseCommandMapper<TRequest,TResponse, TCommand, TEntity>
    where TCommand : class, IRequest<TEntity>
    where TEntity : class, IEntityId
    where TRequest: class
    where TResponse : class

{
    protected new IBaseCommandMapper<TRequest, TResponse, TCommand, TEntity> Map => base.Map;


    protected virtual TCommand ToCommand(TRequest req)
    {
        return Map.ToCommand(req);
    }
    
    protected virtual async Task<TEntity> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        return await handler.HandleAsync(command, ct);
    }

    protected virtual async Task HandleEntityAsync(TEntity entity, CancellationToken ct = default)
    {
        
    }
    protected virtual async Task HandleResponseAsync(TResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }
    public override async Task HandleAsync(TRequest req, CancellationToken ct)
    {
        var command = ToCommand(req);
        var entity = await HandleAsync(command, ct);
        await HandleEntityAsync(entity, ct);
        Response = await Map.FromEntityAsync(entity, ct);
        await HandleResponseAsync(Response, ct);
    }
}