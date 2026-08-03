using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.Services;

public abstract class ServiceBase<T>(IPersistenceService<T> service, IEventPublisher publisher) : IService<T> 
    where T : class, IEntity
{
    protected virtual ISharedEvent? CreateCreatedEvent(T result)
    {
        return null;
    }

    protected virtual ISharedEvent? CreateUpdatedEvent(T result, T existing)
    {
        return null;
    }

    protected virtual ISharedEvent? CreateDeletedEvent(Guid id)
    {
        return null;
    }
    
    public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await service.GetAsync(id, cancellationToken);
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.CreatedAt = DateTime.UtcNow;
        var id = await service.CreateAsync(entity, cancellationToken);
        var result = await service.GetAsync(id, cancellationToken);
        
        if (result is null)
        {
            throw new ServiceException("Failed to retrieve created entity.");
        }
        
        var ev = CreateCreatedEvent(result);

        await PublishEventAsync(ev, cancellationToken);
        
        return result;
    }
    
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        
        var existing = await service.GetAsync(entity.Id, cancellationToken);
        
        if (existing is null)
        {
            throw new ServiceNotFoundException("Entity not found for update.");
        }
        
        entity.UpdatedAt = DateTime.UtcNow;
        await service.UpdateAsync(entity, cancellationToken);
        var result = await service.GetAsync(entity.Id, cancellationToken);
        
        if (result is null)
        {
            throw new ServiceException("Failed to retrieve created entity.");
        }
        
        var ev = CreateUpdatedEvent(result, existing);
        await PublishEventAsync(ev, cancellationToken);
        return result;
    }

    private async Task PublishEventAsync(ISharedEvent? ev, CancellationToken cancellationToken)
    {
        if (ev != null)
        {
            await publisher.PublishAsync(ev, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await service.GetAsync(id, cancellationToken);
        
        if (existing is null)
        {
            throw new ServiceNotFoundException("Entity not found for delete.");
        }
        
        await service.DeleteAsync(id, cancellationToken);
        var ev = CreateDeletedEvent(id);
        await PublishEventAsync(ev, cancellationToken);
    }
}