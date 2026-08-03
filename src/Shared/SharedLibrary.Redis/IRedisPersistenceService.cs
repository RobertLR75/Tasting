using SharedLibrary.Interfaces;

namespace SharedLibrary.Redis;

public interface IRedisPersistenceService<T> : IPersistenceService<T>, IPersistenceSearchService<T> where T : class, IEntity
{
}