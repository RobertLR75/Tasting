using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.Dapper;

public interface IPostgresSqlStorageService<T> : IPersistenceService<T> where T : class, IEntity;
