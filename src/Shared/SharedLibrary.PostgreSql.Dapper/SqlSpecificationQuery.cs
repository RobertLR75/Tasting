using Dapper;

namespace SharedLibrary.PostgreSql.Dapper;

public sealed record SqlSpecificationQuery(string Sql, DynamicParameters Parameters);

public sealed record DapperRelationship(
    string NavigationProperty,
    string TableName,
    string SourceColumn,
    string TargetColumn);
