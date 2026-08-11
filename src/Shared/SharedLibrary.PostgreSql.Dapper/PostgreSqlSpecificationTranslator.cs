using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Ardalis.Specification;
using Dapper;
using SharedLibrary.Interfaces;

namespace SharedLibrary.PostgreSql.Dapper;

public sealed class PostgreSqlSpecificationTranslator<T>(
    string tableName,
    Func<string, string>? columnName = null,
    IReadOnlyCollection<DapperRelationship>? relationships = null,
    string defaultProjection = "root.*")
    where T : class, IEntity
{
    private readonly Func<string, string> _columnName = columnName ?? (name => name);
    private readonly IReadOnlyDictionary<string, DapperRelationship> _relationships =
        (relationships ?? []).ToDictionary(relationship => relationship.NavigationProperty);

    public SqlSpecificationQuery Translate(IPersistenceSpecification<T> specification)
        => TranslateCore(specification, selector: null);

    public SqlSpecificationQuery Translate<TResult>(IPersistenceSpecification<T, TResult> specification)
        => TranslateCore(specification, specification.Selector);

    private SqlSpecificationQuery TranslateCore(ISpecification<T> specification, LambdaExpression? selector)
    {
        ArgumentNullException.ThrowIfNull(specification);

        RejectUnsupportedFeatures(specification);
        var parameters = new DynamicParameters();
        var joins = BuildJoins(specification);
        var projection = selector is null ? defaultProjection : BuildProjection(selector);
        var sql = $"SELECT {projection} FROM {Quote(tableName)} AS root{joins}";

        if (specification.WhereExpressions.Any())
        {
            var predicateWriter = new PredicateWriter(_columnName, parameters);
            var predicates = specification.WhereExpressions
                .Select(expression => predicateWriter.Write(expression.Filter.Body))
                .ToArray();
            sql += $" WHERE {string.Join(" AND ", predicates.Select(predicate => $"({predicate})"))}";
        }

        if (specification.OrderExpressions.Any())
        {
            var orders = specification.OrderExpressions.Select(order =>
            {
                var member = GetRootMember(order.KeySelector.Body);
                var direction = order.OrderType.ToString().Contains("Descending", StringComparison.Ordinal)
                    ? "DESC"
                    : "ASC";
                return $"root.{Quote(_columnName(member.Member.Name))} {direction}";
            });
            sql += $" ORDER BY {string.Join(", ", orders)}";
        }

        if (specification.Take > 0)
        {
            sql += " LIMIT @__take";
            parameters.Add("__take", specification.Take);
        }

        if (specification.Skip > 0)
        {
            sql += " OFFSET @__skip";
            parameters.Add("__skip", specification.Skip);
        }

        return new SqlSpecificationQuery(sql + ";", parameters);
    }

    private string BuildJoins(ISpecification<T> specification)
    {
        var joins = new List<string>();
        foreach (var include in specification.IncludeExpressions)
        {
            var member = GetRootMember(include.LambdaExpression.Body);
            if (!_relationships.TryGetValue(member.Member.Name, out var relationship))
            {
                throw Unsupported($"Relationship '{member.Member.Name}' has no Dapper mapping.");
            }

            var alias = $"rel_{member.Member.Name}";
            joins.Add(
                $" LEFT JOIN {Quote(relationship.TableName)} AS {Quote(alias)}" +
                $" ON root.{Quote(relationship.SourceColumn)} = {Quote(alias)}.{Quote(relationship.TargetColumn)}");
        }

        return string.Concat(joins);
    }

    private string BuildProjection(LambdaExpression selector)
    {
        var body = StripConvert(selector.Body);
        var projectedMembers = body switch
        {
            MemberExpression member => [new Projection(member.Member.Name, member)],
            NewExpression creation => creation.Arguments
                .Select((argument, index) => new Projection(
                    creation.Members?[index].Name
                    ?? selector.ReturnType.GetProperties()[index].Name,
                    argument)),
            MemberInitExpression initialization => initialization.Bindings
                .OfType<MemberAssignment>()
                .Select(assignment => new Projection(assignment.Member.Name, assignment.Expression)),
            _ => throw Unsupported($"Projection node '{body.NodeType}' is not supported.")
        };

        return string.Join(", ", projectedMembers.Select(item =>
        {
            var source = GetRootMember(item.Expression);
            return $"root.{Quote(_columnName(source.Member.Name))} AS {Quote(item.Alias)}";
        }));
    }

    private static void RejectUnsupportedFeatures(ISpecification<T> specification)
    {
        if (specification.PostProcessingAction is not null)
        {
            throw Unsupported("Post-processing is not supported because Dapper queries must be evaluated by PostgreSQL.");
        }

        if (specification.SearchCriterias.Any())
        {
            throw Unsupported("Ardalis Search is not supported; use explicit provider-neutral criteria instead.");
        }

        if (specification.IncludeStrings.Any())
        {
            throw Unsupported("String-based relationships are not supported; use a typed Include expression.");
        }
    }

    private static MemberExpression GetRootMember(Expression expression)
    {
        expression = StripConvert(expression);
        if (expression is MemberExpression member && StripConvert(member.Expression!) is ParameterExpression)
        {
            return member;
        }

        throw Unsupported($"Only direct entity properties are supported here; received '{expression}'.");
    }

    private static Expression StripConvert(Expression expression)
    {
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static string Quote(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || identifier.Any(character => !(char.IsLetterOrDigit(character) || character == '_')))
        {
            throw new InvalidOperationException($"Unsafe PostgreSQL identifier '{identifier}'.");
        }

        return $"\"{identifier}\"";
    }

    private static NotSupportedException Unsupported(string detail)
        => new($"The persistence specification contains an unsupported construct. {detail}");

    private sealed record Projection(string Alias, Expression Expression);

    private sealed class PredicateWriter(Func<string, string> columnName, DynamicParameters parameters)
    {
        private int _parameterIndex;

        public string Write(Expression expression)
        {
            expression = StripConvert(expression);
            return expression switch
            {
                BinaryExpression binary => WriteBinary(binary),
                UnaryExpression { NodeType: ExpressionType.Not } unary => $"NOT ({Write(unary.Operand)})",
                MethodCallExpression call => WriteMethodCall(call),
                MemberExpression member when member.Type == typeof(bool) && IsRootMember(member) =>
                    $"root.{Quote(columnName(member.Member.Name))} = TRUE",
                _ => throw Unsupported($"Criteria node '{expression.NodeType}' is not supported.")
            };
        }

        private string WriteBinary(BinaryExpression binary)
        {
            if (binary.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse)
            {
                var logicalOperator = binary.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
                return $"({Write(binary.Left)}) {logicalOperator} ({Write(binary.Right)})";
            }

            var operators = new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Equal] = "=",
                [ExpressionType.NotEqual] = "<>",
                [ExpressionType.GreaterThan] = ">",
                [ExpressionType.GreaterThanOrEqual] = ">=",
                [ExpressionType.LessThan] = "<",
                [ExpressionType.LessThanOrEqual] = "<="
            };
            if (!operators.TryGetValue(binary.NodeType, out var sqlOperator))
            {
                throw Unsupported($"Binary operator '{binary.NodeType}' is not supported.");
            }

            var member = TryGetRootMember(binary.Left);
            var valueExpression = binary.Right;
            if (member is null)
            {
                member = TryGetRootMember(binary.Right);
                valueExpression = binary.Left;
                sqlOperator = Reverse(sqlOperator);
            }

            if (member is null)
            {
                throw Unsupported("A comparison must contain one direct entity property.");
            }

            var value = Evaluate(valueExpression);
            var column = $"root.{Quote(columnName(member.Member.Name))}";
            if (value is null)
            {
                return sqlOperator switch
                {
                    "=" => $"{column} IS NULL",
                    "<>" => $"{column} IS NOT NULL",
                    _ => throw Unsupported("NULL can only be used with equality comparisons.")
                };
            }

            return $"{column} {sqlOperator} {AddParameter(value)}";
        }

        private string WriteMethodCall(MethodCallExpression call)
        {
            if (call.Object is MemberExpression member && IsRootMember(member) &&
                call.Method.DeclaringType == typeof(string) && call.Arguments.Count == 1)
            {
                var raw = Convert.ToString(Evaluate(call.Arguments[0])) ?? string.Empty;
                var escaped = raw
                    .Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("%", "\\%", StringComparison.Ordinal)
                    .Replace("_", "\\_", StringComparison.Ordinal);

                var pattern = call.Method.Name switch
                {
                    nameof(string.Contains) => $"%{escaped}%",
                    nameof(string.StartsWith) => $"{escaped}%",
                    nameof(string.EndsWith) => $"%{escaped}",
                    _ => throw Unsupported($"String method '{call.Method.Name}' is not supported.")
                };

                return $"root.{Quote(columnName(member.Member.Name))} LIKE {AddParameter(pattern)} ESCAPE '\\'";
            }

            throw Unsupported($"Method call '{call.Method.Name}' is not supported.");
        }

        private string AddParameter(object value)
        {
            var name = $"p{_parameterIndex++}";
            parameters.Add(name, value);
            return $"@{name}";
        }

        private static MemberExpression? TryGetRootMember(Expression expression)
        {
            expression = StripConvert(expression);
            return expression is MemberExpression member && IsRootMember(member) ? member : null;
        }

        private static bool IsRootMember(MemberExpression member)
            => member.Expression is not null && StripConvert(member.Expression) is ParameterExpression;

        private static object? Evaluate(Expression expression)
        {
            expression = StripConvert(expression);
            if (expression is ConstantExpression constant)
            {
                return constant.Value;
            }

            if (expression is MemberExpression member && member.Expression is ConstantExpression closure)
            {
                return member.Member switch
                {
                    FieldInfo field => field.GetValue(closure.Value),
                    PropertyInfo property => property.GetValue(closure.Value),
                    _ => throw Unsupported($"Captured member '{member.Member.Name}' is not supported.")
                };
            }

            throw Unsupported($"Value expression '{expression.NodeType}' is not a constant or captured value.");
        }

        private static string Reverse(string sqlOperator) => sqlOperator switch
        {
            ">" => "<",
            ">=" => "<=",
            "<" => ">",
            "<=" => ">=",
            _ => sqlOperator
        };
    }
}
