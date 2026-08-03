using SharedLibrary.PostgreSql.EntityFramework;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public interface IUserRepository : IPostgresSqlStorageService<User>
{
    Task<User?> GetByEmailNormalizedAsync(string emailNormalized, CancellationToken cancellationToken = default);
}
