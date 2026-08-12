using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.DeactivateUser;

public sealed class DeactivateUserHandler(IPersistenceService<User> users) : IRequestHandler<DeactivateUserCommand, User>
{
    public async Task<User> HandleAsync(DeactivateUserCommand request, CancellationToken ct = default)
    {
        var user = await users.GetAsync(request.Id, ct);
        if (user is null)
        {
            throw new ServiceNotFoundException("Bruker ble ikke funnet.");
        }

        if (!user.IsActive)
        {
            return user;
        }

        if (user.Role == UserRole.Admin &&
            (await users.SearchAsync(new ActiveAdminsSpecification(), ct)).Count == 1)
        {
            throw new ConflictException("The last active admin cannot be deactivated.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.UpdateAsync(user, ct);
        return user;
    }
}
