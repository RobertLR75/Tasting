using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.DeactivateUser;

public sealed class DeactivateUserHandler(IUserRepository userRepository) : IRequestHandler<DeactivateUserCommand, User>
{
    public async Task<User> HandleAsync(DeactivateUserCommand request, CancellationToken ct = default)
    {
        var user = await userRepository.GetAsync(request.Id, ct);
        if (user is null)
        {
            throw new ServiceNotFoundException("Bruker ble ikke funnet.");
        }

        if (!user.IsActive)
        {
            return user;
        }

        user.IsActive = false;
        await userRepository.UpdateAsync(user, ct);
        return user;
    }
}
