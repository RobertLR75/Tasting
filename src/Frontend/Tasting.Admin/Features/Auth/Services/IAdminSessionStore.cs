using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.Features.Auth.Services;

public interface IAdminSessionStore
{
    Task<StoredAdminSession?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(StoredAdminSession session, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
