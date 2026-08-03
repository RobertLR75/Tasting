using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.EntityFramework;

public interface IPostgresSqlStorageService<T> : IPersistenceService<T>, IPersistenceSearchService<T> where T : class, IEntity;