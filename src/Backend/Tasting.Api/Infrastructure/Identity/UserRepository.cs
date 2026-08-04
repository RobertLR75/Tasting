using Microsoft.EntityFrameworkCore;
using SharedLibrary.PostgreSql.EntityFramework;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class UserRepository(UsersDbContext context) : EntityFrameworkPostgresSqlStorageBase<User>(context), IUserRepository
{
    public Task<User?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken cancellationToken = default)
    {
        var normalized = emailNormalized.Trim().ToLowerInvariant();
        return context.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.EmailNormalized == normalized, cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
    {
        return context.Users.AsNoTracking()
            .CountAsync(user => user.IsActive && user.Role == UserRole.Admin, cancellationToken);
    }
}
