namespace SharedLibrary.Interfaces;

public interface IService<T> where T : class, IEntity
{
    public Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken=default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken=default);
}