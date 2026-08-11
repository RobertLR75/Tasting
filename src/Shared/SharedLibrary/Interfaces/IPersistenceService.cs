namespace SharedLibrary.Interfaces;

public interface IPersistenceService<T> where T : class, IEntity
{
    public Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(T entity, CancellationToken cancellationToken=default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    
    public Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default);
    public Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default);
    public Task<List<TResult>> SearchAsync<TResult>(
        IPersistenceSpecification<T, TResult> specification,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException($"{GetType().Name} does not support projection specifications.");
}
