using System.Linq.Expressions;
using Ardalis.Specification;
using MongoDB.Driver;
using SharedLibrary.Interfaces;

namespace SharedLibrary.MongoDB;

/// <summary>
/// Abstrakt basisklasse for MongoDB-basert storage av entiteter.
/// Implementerer CRUD-operasjoner med MongoDB som database.
/// Bruker MongoDB collections for å lagre entiteter og støtter queries, indexes og aggregations.
/// </summary>
/// <typeparam name="T">Type entitet som skal lagres (må implementere IEntity)</typeparam>

public abstract class MongoDbStorageBase<T> : IMongoDbStorageService<T>
    where T : class, IEntity
{
    //private readonly IClock _clock;
    
    /// <summary>
    /// MongoDB collection som brukes for å lagre entiteter.
    /// </summary>
    private readonly IMongoCollection<T> _collection;

    /// <summary>
    /// Navn på MongoDB collection (f.eks. "persons").
    /// Må implementeres av avledede klasser.
    /// </summary>
    protected abstract string CollectionName { get; }

    /// <summary>
    /// Konstruktør som initialiserer MongoDB collection.
    /// </summary>
    /// <param name="database">MongoDB database</param>
    /// <param name="clock">Clock for tidsstempler</param>
    protected MongoDbStorageBase(IMongoDatabase database)
    {
        _collection = database.GetCollection<T>(CollectionName);
    }

    /// <summary>
    /// Oppretter en ny entitet i MongoDB.
    /// Genererer ny ID hvis den er tom, setter CreatedAt-tidspunkt og lagrer i database.
    /// </summary>
    /// <param name="entity">Entitet som skal opprettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID på den opprettede entiteten</returns>
    public virtual async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        // Generer ny Version 7 GUID hvis ID er tom, ellers behold eksisterende ID
        entity.Id = entity.Id == Guid.Empty ? Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = DateTimeOffset.UtcNow;
        
        // Lagre entitet i MongoDB
        await _collection.InsertOneAsync(entity, null, cancellationToken);
        
        return entity.Id;
    }

    /// <summary>
    /// Oppdaterer en eksisterende entitet i MongoDB.
    /// Setter UpdatedAt-tidspunkt og erstatter hele dokumentet.
    /// </summary>
    /// <param name="entity">Entitet med oppdaterte verdier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        
        // Erstatt hele dokumentet i MongoDB
        var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions(), cancellationToken);
    }

    /// <summary>
    /// Sletter en entitet fra MongoDB basert på ID.
    /// </summary>
    /// <param name="id">ID på entiteten som skal slettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Henter en enkelt entitet fra MongoDB basert på ID.
    /// </summary>
    /// <param name="id">ID på entiteten som skal hentes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entitet hvis funnet, null ellers</returns>
    public virtual async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id);
        var cursor = await _collection.FindAsync(filter, null, cancellationToken);
        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<T>> SearchAsync(SearchFilter<T> filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var mongoFilter = BuildMongoFilter(filter);
        var sortDefinition = BuildMongoSort(filter.SortFields);

        var options = new FindOptions<T>
        {
            Sort = sortDefinition
        };

        var cursor = await _collection.FindAsync(mongoFilter, options, cancellationToken);
        return await cursor.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Searches entities using an Ardalis specification. Fetches all documents and applies the
    /// specification in-memory. Supported features: Where, Order, Take, Skip.
    /// Includes are effectively no-op (document model).
    /// </summary>
    public virtual async Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var cursor = await _collection.FindAsync(Builders<T>.Filter.Empty, cancellationToken: cancellationToken);
        var allEntities = await cursor.ToListAsync(cancellationToken);

        return InMemorySpecificationEvaluator.Default.Evaluate(allEntities, specification).ToList();
    }

    public async Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var results = await SearchAsync(specification, cancellationToken);
        return results.FirstOrDefault()
               ?? throw new InvalidOperationException("Entity matching specification was not found.");
    }

    private static FilterDefinition<T> BuildMongoFilter(SearchFilter<T> filter)
    {
        if (filter.Parameters.Count == 0)
            return Builders<T>.Filter.Empty;

        var filters = filter.Parameters
            .Select(c =>
            {
                var fieldName = ExtractPropertyName(c.FieldSelector);
                return Builders<T>.Filter.Eq(fieldName, c.Value);
            })
            .ToList();

        return filter.LogicalOperator == SearchLogicalOperator.And
            ? Builders<T>.Filter.And(filters)
            : Builders<T>.Filter.Or(filters);
    }

    private static SortDefinition<T>? BuildMongoSort(List<SearchSortCriterion<T>> sortFields)
    {
        if (sortFields.Count == 0) return null;

        var builder = Builders<T>.Sort;
        var sorts = sortFields.Select(s =>
        {
            var fieldName = ExtractPropertyName(s.FieldSelector);
            return s.Direction == SearchSortDirection.Ascending
                ? builder.Ascending(fieldName)
                : builder.Descending(fieldName);
        });

        return builder.Combine(sorts);
    }

    private static string ExtractPropertyName(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException("Selector must reference a property.", nameof(selector));
    }

    #region Query Helpers

    /// <summary>
    /// Protected helper for å utføre egendefinerte queries mot MongoDB.
    /// Kan brukes av avledede klasser for spesialiserte søk.
    /// </summary>
    /// <param name="filter">MongoDB filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liste med matchende entiteter</returns>
    protected async Task<List<T>> FindAsync(FilterDefinition<T> filter, CancellationToken cancellationToken)
    {
        var cursor = await _collection.FindAsync(filter, null, cancellationToken);
        return await cursor.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Protected helper for å telle dokumenter i collection.
    /// </summary>
    /// <param name="filter">MongoDB filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Antall dokumenter som matcher filter</returns>
    protected async Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken)
    {
        return await _collection.CountDocumentsAsync(filter, null, cancellationToken);
    }

    /// <summary>
    /// Gir tilgang til den underliggende MongoDB collection for avanserte operasjoner.
    /// </summary>
    protected IMongoCollection<T> Collection => _collection;

    #endregion

    #region Index Management

    /// <summary>
    /// Oppretter indexes på MongoDB collection.
    /// Skal kalles ved oppstart for å sikre optimal query-ytelse.
    /// Standard: index på Id og CreatedAt.
    /// </summary>
    public virtual async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var indexKeysDefinition = Builders<T>.IndexKeys
            .Ascending(e => e.Id)
            .Ascending(e => e.CreatedAt);

        var indexModel = new CreateIndexModel<T>(indexKeysDefinition, new CreateIndexOptions
        {
            Name = "idx_id_created"
        });

        await _collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }

    #endregion
}
