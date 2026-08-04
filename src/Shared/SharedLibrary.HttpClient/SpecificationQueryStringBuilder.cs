using System.Globalization;
using System.Linq.Expressions;
using SharedLibrary.Interfaces;

namespace SharedLibrary.HttpClient;

public static class SpecificationQueryStringBuilder
{
    public static string BuildQueryString<T>(IApiSpecification<T>? specification)
    {
        if (specification is null)
        {
            return string.Empty;
        }

        var queryParameters = new List<KeyValuePair<string, string>>();

        var whereFilters = specification.WhereExpressions
            .Select(where => SerializePredicate(where.Filter.Body))
            .ToList();

        if (whereFilters.Count > 0)
        {
            queryParameters.Add(new KeyValuePair<string, string>("filter", string.Join(" and ", whereFilters)));
        }

        foreach (var search in specification.SearchCriterias)
        {
            var memberPath = GetMemberPath(search.Selector);
            var searchValue = $"{memberPath}~{FormatValue(search.SearchTerm)}";

            if (search.SearchGroup != 1)
            {
                searchValue += $";group={search.SearchGroup.ToString(CultureInfo.InvariantCulture)}";
            }

            queryParameters.Add(new KeyValuePair<string, string>("search", searchValue));
        }

        queryParameters.AddRange(from order in specification.OrderExpressions
            let direction = order.OrderType.ToString().Contains("Descending", StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc"
            select new KeyValuePair<string, string>("sort", $"{GetMemberPath(order.KeySelector)}:{direction}"));

        if (specification.Skip is int skip)
        {
            queryParameters.Add(new KeyValuePair<string, string>("skip", skip.ToString(CultureInfo.InvariantCulture)));
        }

        if (specification.Take is var take)
        {
            queryParameters.Add(new KeyValuePair<string, string>("take", take.ToString(CultureInfo.InvariantCulture)));
        }

        return string.Join("&", queryParameters.Select(static parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
    }

    private static string SerializePredicate(Expression expression)
    {
        expression = UnwrapConvert(expression);

        return expression switch
        {
            BinaryExpression binary when IsLogical(binary.NodeType)
                => $"({SerializePredicate(binary.Left)} {GetLogicalOperator(binary.NodeType)} {SerializePredicate(binary.Right)})",
            BinaryExpression binary when TrySerializeComparison(binary, out var comparison)
                => comparison,
            MethodCallExpression methodCall => SerializeMethodCall(methodCall),
            MemberExpression member when member.Type == typeof(bool)
                => $"{GetMemberPath(member)} eq true",
            UnaryExpression { NodeType: ExpressionType.Not } unary
                => $"not ({SerializePredicate(unary.Operand)})",
            ConstantExpression constant when constant.Type == typeof(bool)
                => FormatValue(constant.Value),
            _ => throw new NotSupportedException($"The filter expression '{expression}' is not supported for API query translation.")
        };
    }

    private static bool TrySerializeComparison(BinaryExpression binary, out string comparison)
    {
        if (!IsComparison(binary.NodeType))
        {
            comparison = string.Empty;
            return false;
        }

        if (TryGetMemberPath(binary.Left, out var leftPath))
        {
            comparison = $"{leftPath} {GetComparisonOperator(binary.NodeType)} {FormatValue(Evaluate(binary.Right))}";
            return true;
        }

        if (TryGetMemberPath(binary.Right, out var rightPath))
        {
            comparison = $"{rightPath} {GetComparisonOperator(Reverse(binary.NodeType))} {FormatValue(Evaluate(binary.Left))}";
            return true;
        }

        comparison = string.Empty;
        return false;
    }

    private static string SerializeMethodCall(MethodCallExpression methodCall)
    {
        if (methodCall.Method.DeclaringType != typeof(string))
        {
            throw new NotSupportedException($"The method '{methodCall.Method.Name}' is not supported for API query translation.");
        }

        return methodCall.Method.Name switch
        {
            nameof(string.Contains) => SerializeStringMethod("contains", methodCall),
            nameof(string.StartsWith) => SerializeStringMethod("startsWith", methodCall),
            nameof(string.EndsWith) => SerializeStringMethod("endsWith", methodCall),
            _ => throw new NotSupportedException($"The method '{methodCall.Method.Name}' is not supported for API query translation.")
        };
    }

    private static string SerializeStringMethod(string methodName, MethodCallExpression methodCall)
    {
        if (methodCall.Object is null)
        {
            throw new NotSupportedException($"The method '{methodCall.Method.Name}' must be called on a string property.");
        }

        var memberPath = GetMemberPath(methodCall.Object);
        var value = methodCall.Arguments.Count > 0 ? Evaluate(methodCall.Arguments[0]) : null;
        return $"{methodName}({memberPath}, {FormatValue(value)})";
    }

    private static bool TryGetMemberPath(Expression expression, out string memberPath)
    {
        try
        {
            memberPath = GetMemberPath(expression);
            return true;
        }
        catch (NotSupportedException)
        {
            memberPath = string.Empty;
            return false;
        }
    }

    private static string GetMemberPath(LambdaExpression expression) => GetMemberPath(expression.Body);

    private static string GetMemberPath(Expression expression)
    {
        expression = UnwrapConvert(expression);

        var segments = new Stack<string>();
        var current = expression;

        while (current is MemberExpression memberExpression)
        {
            segments.Push(memberExpression.Member.Name);
            current = UnwrapConvert(memberExpression.Expression!);
        }

        if (current is ParameterExpression && segments.Count > 0)
        {
            return string.Join('.', segments);
        }

        throw new NotSupportedException($"The member access '{expression}' is not supported for API query translation.");
    }

    private static object? Evaluate(Expression expression)
    {
        expression = UnwrapConvert(expression);

        if (expression is ConstantExpression constant)
        {
            return constant.Value;
        }

        var objectMember = Expression.Convert(expression, typeof(object));
        var lambda = Expression.Lambda<Func<object?>>(objectMember);
        return lambda.Compile().Invoke();
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static bool IsLogical(ExpressionType nodeType)
        => nodeType is ExpressionType.AndAlso or ExpressionType.OrElse;

    private static bool IsComparison(ExpressionType nodeType)
        => nodeType is ExpressionType.Equal
            or ExpressionType.NotEqual
            or ExpressionType.GreaterThan
            or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan
            or ExpressionType.LessThanOrEqual;

    private static string GetLogicalOperator(ExpressionType nodeType) => nodeType switch
    {
        ExpressionType.AndAlso => "and",
        ExpressionType.OrElse => "or",
        _ => throw new NotSupportedException($"The logical operator '{nodeType}' is not supported for API query translation.")
    };

    private static string GetComparisonOperator(ExpressionType nodeType) => nodeType switch
    {
        ExpressionType.Equal => "eq",
        ExpressionType.NotEqual => "ne",
        ExpressionType.GreaterThan => "gt",
        ExpressionType.GreaterThanOrEqual => "gte",
        ExpressionType.LessThan => "lt",
        ExpressionType.LessThanOrEqual => "lte",
        _ => throw new NotSupportedException($"The comparison operator '{nodeType}' is not supported for API query translation.")
    };

    private static ExpressionType Reverse(ExpressionType nodeType) => nodeType switch
    {
        ExpressionType.GreaterThan => ExpressionType.LessThan,
        ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
        ExpressionType.LessThan => ExpressionType.GreaterThan,
        ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
        _ => nodeType
    };

    private static string FormatValue(object? value) => value switch
    {
        null => "null",
        string text => $"'{text.Replace("'", "''", StringComparison.Ordinal)}'",
        Guid guid => $"'{guid:D}'",
        DateTime dateTime => $"'{dateTime.ToUniversalTime():O}'",
        DateTimeOffset dateTimeOffset => $"'{dateTimeOffset.ToUniversalTime():O}'",
        bool boolean => boolean ? "true" : "false",
        Enum enumValue => $"'{enumValue}'",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => $"'{value}'"
    };
}
