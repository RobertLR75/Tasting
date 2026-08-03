namespace SharedLibrary.Interfaces;

public interface IPersistenceSearchService<T> where T : class, IEntity
{
    public Task<List<T>> SearchAsync(SearchFilter<T> filter, CancellationToken cancellationToken = default);
}
