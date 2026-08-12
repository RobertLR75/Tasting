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
        var includedRelationships = ResolveRelationships(specification);
        if (selector is not null && includedRelationships.Count > 0)
        {
            throw Unsupported("Relationship includes cannot be combined with projection specifications.");
        }

        var joins = BuildJoins(includedRelationships);
        var projection = selector is null
            ? BuildEntityProjection(includedRelationships)
            : BuildProjection(selector);
        var whereClause = BuildWhereClause(specification, parameters);
        var orderClause = BuildOrderClause(specification);
        var pagingClause = BuildPagingClause(specification, parameters);

        string sql;
        if (includedRelationships.Count > 0 && !string.IsNullOrEmpty(pagingClause))
        {
            var pagedRoots = $"SELECT root.* FROM {Quote(tableName)} AS root{whereClause}{orderClause}{pagingClause}";
            sql = $"SELECT {projection} FROM ({pagedRoots}) AS root{joins}{orderClause}";
        }
        else
        {
            sql = $"SELECT {projection} FROM {Quote(tableName)} AS root{joins}{whereClause}{orderClause}{pagingClause}";
        }

        return new SqlSpecificationQuery(sql + ";", parameters, includedRelationships);
    }

    private string BuildWhereClause(ISpecification<T> specification, DynamicParameters parameters)
    {
        if (!specification.WhereExpressions.Any())
        {
            return string.Empty;
        }

        var predicateWriter = new PredicateWriter(_columnName, parameters);
        var predicates = specification.WhereExpressions
            .Select(expression => predicateWriter.Write(expression.Filter.Body))
            .ToArray();
        return $" WHERE {string.Join(" AND ", predicates.Select(predicate => $"({predicate})"))}";
    }

    private string BuildOrderClause(ISpecification<T> specification)
    {
        if (!specification.OrderExpressions.Any())
        {
            return string.Empty;
        }

        var orders = specification.OrderExpressions.Select(order =>
        {
            var member = GetRootMember(order.KeySelector.Body);
            var direction = order.OrderType.ToString().Contains("Descending", StringComparison.Ordinal)
                ? "DESC"
                : "ASC";
            return $"root.{Quote(_columnName(member.Member.Name))} {direction}";
        });
        return $" ORDER BY {string.Join(", ", orders)}";
    }

    private static string BuildPagingClause(ISpecification<T> specification, DynamicParameters parameters)
    {
        var paging = string.Empty;
        if (specification.Take > 0)
        {
            paging += " LIMIT @__take";
            parameters.Add("__take", specification.Take);
        }

        if (specification.Skip > 0)
        {
            paging += " OFFSET @__skip";
            parameters.Add("__skip", specification.Skip);
        }

        return paging;
    }

    private IReadOnlyList<DapperRelationship> ResolveRelationships(ISpecification<T> specification)
    {
        var included = new List<DapperRelationship>();
        foreach (var include in specification.IncludeExpressions)
        {
            var member = GetRootMember(include.LambdaExpression.Body);
            if (!_relationships.TryGetValue(member.Member.Name, out var relationship))
            {
                throw Unsupported($"Relationship '{member.Member.Name}' has no Dapper mapping.");
            }

            included.Add(relationship);
        }

        return included;
    }

    private static string BuildJoins(IReadOnlyList<DapperRelationship> relationships)
    {
        return string.Concat(relationships.Select(relationship =>
        {
            var alias = RelationshipAlias(relationship);
            return
                $" LEFT JOIN {Quote(relationship.TableName)} AS {Quote(alias)}" +
                $" ON root.{Quote(relationship.SourceColumn)} = {Quote(alias)}.{Quote(relationship.TargetColumn)}";
        }));
    }

    private string BuildEntityProjection(IReadOnlyList<DapperRelationship> relationships)
    {
        var projections = new List<string> { defaultProjection };
        projections.AddRange(relationships.Select(BuildRelationshipProjection));
        return string.Join(", ", projections);
    }

    private static string BuildRelationshipProjection(DapperRelationship relationship)
    {
        var alias = RelationshipAlias(relationship);
        var properties = relationship.RelatedType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
            .Where(property => IsScalar(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType));

        var projection = string.Join(", ", properties.Select(property =>
            $"{Quote(alias)}.{Quote(relationship.ColumnName(property.Name))} AS {Quote(property.Name)}"));

        if (string.IsNullOrWhiteSpace(projection))
        {
            throw Unsupported($"Relationship '{relationship.NavigationProperty}' has no scalar properties to materialize.");
        }

        return projection;
    }

    private static bool IsScalar(Type type)
        => type.IsEnum
           || type == typeof(string)
           || type == typeof(Guid)
           || type == typeof(DateTimeOffset)
           || type == typeof(DateTime)
           || type == typeof(bool)
           || type == typeof(int)
           || type == typeof(long)
           || type == typeof(decimal)
           || type == typeof(double)
           || type == typeof(float)
           || type == typeof(short)
           || type == typeof(byte);

    private static string RelationshipAlias(DapperRelationship relationship)
        => $"rel_{relationship.NavigationProperty}";

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
            if (TryGetStringMember(call.Object, out var member, out var lowerCase) &&
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

                var column = $"root.{Quote(columnName(member.Member.Name))}";
                if (lowerCase)
                {
                    column = $"LOWER({column})";
                }

                return $"{column} LIKE {AddParameter(pattern)} ESCAPE '\\'";
            }

            throw Unsupported($"Method call '{call.Method.Name}' is not supported.");
        }

        private static bool TryGetStringMember(
            Expression? expression,
            out MemberExpression member,
            out bool lowerCase)
        {
            expression = expression is null ? null : StripConvert(expression);
            if (expression is MemberExpression direct && IsRootMember(direct))
            {
                member = direct;
                lowerCase = false;
                return true;
            }

            if (expression is MethodCallExpression
                {
                    Method.Name: nameof(string.ToLower),
                    Arguments.Count: 0,
                    Object: MemberExpression lowered
                } && IsRootMember(lowered))
            {
                member = lowered;
                lowerCase = true;
                return true;
            }

            member = null!;
            lowerCase = false;
            return false;
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
