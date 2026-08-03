using System.Linq.Expressions;

namespace SharedLibrary.Interfaces;

public enum SearchLogicalOperator
{
    And,
    Or
}

public enum SearchSortDirection
{
    Ascending,
    Descending
}

public sealed class SearchFilter<T>
{
    public List<SearchFilterCriterion<T>> Parameters { get; init; } = [];
    public SearchLogicalOperator LogicalOperator { get; init; } = SearchLogicalOperator.And;
    public List<SearchSortCriterion<T>> SortFields { get; init; } = [];
}

public sealed class SearchFilterCriterion<T>(Expression<Func<T, object?>> fieldSelector, object? value)
{
    public Expression<Func<T, object?>> FieldSelector { get; } = EnsurePropertySelector(fieldSelector);
    public object? Value { get; } = value;

    private static Expression<Func<T, object?>> EnsurePropertySelector(Expression<Func<T, object?>> selector)
    {
        if (!IsPropertySelector(selector))
        {
            throw new ArgumentException("Field selector must reference a property on the entity type.", nameof(selector));
        }

        return selector;
    }

    private static bool IsPropertySelector(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;

        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression member && member.Member.MemberType == System.Reflection.MemberTypes.Property;
    }
}

public sealed class SearchSortCriterion<T>(Expression<Func<T, object?>> fieldSelector, SearchSortDirection direction = SearchSortDirection.Ascending)
{
    public Expression<Func<T, object?>> FieldSelector { get; } = EnsurePropertySelector(fieldSelector);
    public SearchSortDirection Direction { get; } = direction;

    private static Expression<Func<T, object?>> EnsurePropertySelector(Expression<Func<T, object?>> selector)
    {
        if (!IsPropertySelector(selector))
        {
            throw new ArgumentException("Field selector must reference a property on the entity type.", nameof(selector));
        }

        return selector;
    }

    private static bool IsPropertySelector(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;

        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression member && member.Member.MemberType == System.Reflection.MemberTypes.Property;
    }
}
