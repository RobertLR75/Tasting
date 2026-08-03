using SharedLibrary.Interfaces;

namespace SharedLibrary.MongoDB;

public interface IMongoDbStorageService<T> : IPersistenceService<T>, IPersistenceSearchService<T> where T : class, IEntity;