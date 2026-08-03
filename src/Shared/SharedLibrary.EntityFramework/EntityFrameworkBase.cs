using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SharedLibrary.Interfaces;

namespace SharedLibrary.EntityFramework;

public abstract class EntityFrameworkBase<T>(DbContext context)
    where T : class, IEntity
{
    protected readonly DbContext Context = context;
    public IDbContextTransaction? Transaction { get; set; }
    protected DbSet<T> DbSet { get; } = context.Set<T>();

    protected async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        if (Transaction is not null)
        {
            await action();
            return;
        }

        // Some providers (like the InMemory provider) do not support transactions.
        // Beginning a transaction on those providers raises a TransactionIgnoredWarning
        // which may be configured to throw. Detect the provider and skip creating a
        // transaction when transactions are not supported.
        var providerName = Context.Database.ProviderName ?? string.Empty;
        var isInMemory = providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);
        if (isInMemory)
        {
            await action();
            return;
        }

        await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    protected static IQueryable<T> ApplySorting(IQueryable<T> query, List<SearchSortCriterion<T>> sortFields)
    {
        if (sortFields.Count == 0) return query;

        IOrderedQueryable<T>? ordered = null;

        for (var i = 0; i < sortFields.Count; i++)
        {
            var sort = sortFields[i];
            var keySelector = sort.FieldSelector;

            if (i == 0)
            {
                ordered = sort.Direction == SearchSortDirection.Ascending
                    ? query.OrderBy(keySelector)
                    : query.OrderByDescending(keySelector);
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
    protected static Expression<Func<T, bool>> BuildFilterPredicate(SearchFilter<T> filter)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? combined = null;

        foreach (var criterion in filter.Parameters)
        {
            var memberBody = ExtractMemberExpression(criterion.FieldSelector.Body);
            var member = Expression.MakeMemberAccess(parameter, ((MemberExpression)memberBody).Member);
            var value = Expression.Constant(criterion.Value, memberBody.Type);
            var equality = Expression.Equal(member, value);

            combined = combined is null
                ? equality
                : filter.LogicalOperator == SearchLogicalOperator.And
                    ? Expression.AndAlso(combined, equality)
                    : Expression.OrElse(combined, equality);
        }

        return Expression.Lambda<Func<T, bool>>(combined!, parameter);
    }
    
    private static Expression ExtractMemberExpression(Expression body)
    {
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            return unary.Operand;
        return body;
    }
}