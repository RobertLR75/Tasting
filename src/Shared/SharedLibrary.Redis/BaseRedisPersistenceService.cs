using System.Text.Json;
using Ardalis.Specification;
using Microsoft.Extensions.Caching.Distributed;
using SharedLibrary.Interfaces;

namespace SharedLibrary.Redis;

public abstract class BaseRedisPersistenceService<T>(IDistributedCache cache) : IRedisPersistenceService<T>
    where T : class, IEntity
{
    protected readonly IDistributedCache Cache = cache;
    public abstract string Name { get; }

    #region Index Management
    private async Task UpdateIndexAsync(string id, CancellationToken cancellationToken)
    {
        var listJson = await Cache.GetStringAsync(Name + ":index", token: cancellationToken) ?? "[]";
        if (listJson == "") listJson = "[]";
        
        var ids = JsonSerializer.Deserialize<List<string>>(listJson) ?? [];
        if (!ids.Contains(id))
        {
            ids.Add(id);
            await Cache.SetStringAsync(Name + ":index", JsonSerializer.Serialize(ids), token: cancellationToken);
        }
    }

    private async Task DeleteIndexAsync(string id, CancellationToken cancellationToken)
    {
        var listJson = await Cache.GetStringAsync(Name + ":index", token: cancellationToken) ?? "[]";
        if (listJson == "") listJson = "[]";
        var ids = JsonSerializer.Deserialize<List<string>>(listJson) ?? [];
        if (ids.Contains(id))
        {
            ids.Remove(id);
            await Cache.SetStringAsync(Name + "index", JsonSerializer.Serialize(ids), token: cancellationToken);
        }
    }

    protected async Task<List<string>> GetIndexAsync(CancellationToken cancellationToken)
    {
        var listJson = await Cache.GetStringAsync(Name + ":index", token: cancellationToken) ?? "[]";
        if (listJson == "") listJson = "[]";
        var ids = JsonSerializer.Deserialize<List<string>>(listJson) ?? [];
        return ids;
    }
    
    #endregion
    
    public virtual async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.Id = entity.Id == Guid.Empty ? entity.Id = Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = DateTimeOffset.UtcNow;
        var json = JsonSerializer.Serialize(entity);
        await Cache.SetStringAsync(Name + $":{entity.Id}", json, token: cancellationToken);

        await UpdateIndexAsync(entity.Id.ToString(), cancellationToken);
        return entity.Id;
    }

    public  virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        var id = entity.Id;
        var json = JsonSerializer.Serialize(entity);
        await Cache.SetStringAsync(Name + $":{id}", json, token: cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await Cache.RemoveAsync(Name + $":{id}",token: cancellationToken);
    }
    
    public virtual async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var cachedItems = await Cache.GetStringAsync(Name + $":{id}", token: cancellationToken);
        return cachedItems != null ? JsonSerializer.Deserialize<T>(cachedItems) : null;
    }

    public virtual async Task<List<T>> SearchAsync(SearchFilter<T> filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var ids = await GetIndexAsync(cancellationToken);
        var entities = new List<T>();

        foreach (var id in ids)
        {
            var json = await Cache.GetStringAsync(Name + $":{id}", token: cancellationToken);
            if (json is not null)
            {
                var entity = JsonSerializer.Deserialize<T>(json);
                if (entity is not null)
                    entities.Add(entity);
            }
        }

        IEnumerable<T> results = entities;

        if (filter.Parameters.Count > 0)
        {
            var predicate = CompileFilterPredicate(filter);
            results = results.Where(predicate);
        }

        results = ApplyInMemorySorting(results, filter.SortFields);

        return results.ToList();
    }

    /// <summary>
    /// Searches entities using an Ardalis specification. Loads all cached entities and applies the
    /// specification in-memory. Supported features: Where, Order, Take, Skip.
    /// </summary>
    public virtual async Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var ids = await GetIndexAsync(cancellationToken);
        var entities = new List<T>();

        foreach (var id in ids)
        {
            var json = await Cache.GetStringAsync(Name + $":{id}", token: cancellationToken);
            if (json is not null)
            {
                var entity = JsonSerializer.Deserialize<T>(json);
                if (entity is not null)
                    entities.Add(entity);
            }
        }

        return InMemorySpecificationEvaluator.Default.Evaluate(entities, specification).ToList();
    }

    public virtual async Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var results = await SearchAsync(specification, cancellationToken);
        return results.FirstOrDefault()
               ?? throw new InvalidOperationException("Entity matching specification was not found.");
    }

    private static Func<T, bool> CompileFilterPredicate(SearchFilter<T> filter)
    {
        var compiledCriteria = filter.Parameters
            .Select(c =>
            {
                var selector = c.FieldSelector.Compile();
                return new Func<T, bool>(entity => Equals(selector(entity), c.Value));
            })
            .ToList();

        return filter.LogicalOperator == SearchLogicalOperator.And
            ? entity => compiledCriteria.All(p => p(entity))
            : entity => compiledCriteria.Any(p => p(entity));
    }

    private static IEnumerable<T> ApplyInMemorySorting(IEnumerable<T> source, List<SearchSortCriterion<T>> sortFields)
    {
        if (sortFields.Count == 0) return source;

        IOrderedEnumerable<T>? ordered = null;

        for (var i = 0; i < sortFields.Count; i++)
        {
            var sort = sortFields[i];
            var keySelector = sort.FieldSelector.Compile();

            if (i == 0)
            {
                ordered = sort.Direction == SearchSortDirection.Ascending
                    ? source.OrderBy(keySelector)
                    : source.OrderByDescending(keySelector);
            }
            else
            {
                ordered = sort.Direction == SearchSortDirection.Ascending
                    ? ordered!.ThenBy(keySelector)
                    : ordered!.ThenByDescending(keySelector);
            }
        }

        return ordered!;
    }
}
