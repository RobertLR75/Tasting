namespace SharedLibrary.Interfaces;

public interface IPersistenceService<T> where T : class, IEntity
{
    public static string Name { get; }
    public Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(T entity, CancellationToken cancellationToken=default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    
    public Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default);
    public Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default);
}

public interface IPersistenceService<T, TResult> where T : class, IPersistenceService<T>, IEntity
{
    public Task<List<TResult>> SearchAsync(IPersistenceSpecification<T, TResult> specification, CancellationToken cancellationToken = default);    
}
