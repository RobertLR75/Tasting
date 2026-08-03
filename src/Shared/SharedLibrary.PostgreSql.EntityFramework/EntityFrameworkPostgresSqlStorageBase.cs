using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.EntityFramework;
using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.EntityFramework;

public abstract class EntityFrameworkPostgresSqlStorageBase<T>(DbContext context) : EntityFrameworkBase<T>(context), IPostgresSqlStorageService<T>
    where T : class, IEntity
{
    public virtual async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = DateTimeOffset.UtcNow;

        await ExecuteInTransactionAsync(async () =>
        {
            await DbSet.AddAsync(entity, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return entity.Id;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await ExecuteInTransactionAsync(async () =>
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            var entity = await DbSet.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity is not null)
            {
                DbSet.Remove(entity);
                await Context.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken);
    }

    public virtual Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<List<T>> SearchAsync(SearchFilter<T> filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<T> query = DbSet.AsNoTracking();

        if (filter.Parameters.Count > 0)
        {
            var predicate = BuildFilterPredicate(filter);
            query = query.Where(predicate);
        }

        query = ApplySorting(query, filter.SortFields);

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<T>> SearchAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.Default.GetQuery(DbSet.AsNoTracking(), specification);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> GetAsync(IPersistenceSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var query = SpecificationEvaluator.Default.GetQuery(DbSet.AsNoTracking(), specification);
        return await query.FirstOrDefaultAsync(cancellationToken)
               ?? throw new InvalidOperationException("Entity matching specification was not found.");
    }
}
