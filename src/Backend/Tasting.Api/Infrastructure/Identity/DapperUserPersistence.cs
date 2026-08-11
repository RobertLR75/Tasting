using System.Data;
using System.Data.Common;
using Dapper;
using SharedLibrary.PostgreSql.Dapper;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class DapperUserPersistence(DbConnection connection)
    : PostgresSqlDapperStorageBase<User>(connection)
{
    protected override string TableName => "users";

    protected override string MapPropertyToColumn(string propertyName) => propertyName switch
    {
        nameof(User.Id) => "id",
        nameof(User.Email) => "email",
        nameof(User.EmailNormalized) => "email_normalized",
        nameof(User.FirstName) => "first_name",
        nameof(User.LastName) => "last_name",
        nameof(User.PasswordHash) => "password_hash",
        nameof(User.IsActive) => "is_active",
        nameof(User.Role) => "role",
        nameof(User.CreatedAt) => "created_at_utc",
        nameof(User.UpdatedAt) => "updated_at_utc",
        _ => throw new InvalidOperationException($"User property '{propertyName}' has no PostgreSQL mapping.")
    };

    protected override object GetCommandParameters(User user) => new
    {
        user.Id,
        user.Email,
        user.EmailNormalized,
        user.FirstName,
        user.LastName,
        user.PasswordHash,
        Role = user.Role.ToString(),
        user.IsActive,
        user.CreatedAt,
        user.UpdatedAt
    };
}

internal sealed class UserRoleTypeHandler : SqlMapper.TypeHandler<UserRole>
{
    public override void SetValue(IDbDataParameter parameter, UserRole value)
        => parameter.Value = value.ToString();

    public override UserRole Parse(object value)
        => Enum.Parse<UserRole>(Convert.ToString(value)!, ignoreCase: false);
}
