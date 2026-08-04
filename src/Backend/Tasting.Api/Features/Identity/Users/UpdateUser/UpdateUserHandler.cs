using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.UpdateUser;

public sealed class UpdateUserHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, User>
{
    public async Task<User> HandleAsync(UpdateUserCommand request, CancellationToken ct = default)
    {
        var user = await userRepository.GetAsync(request.Id, ct);
        if (user is null)
        {
            throw new ServiceNotFoundException("Bruker ble ikke funnet.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        if (!string.Equals(user.EmailNormalized, normalizedEmail, StringComparison.Ordinal))
        {
            var existing = await userRepository.GetByEmailNormalizedAsync(normalizedEmail, ct);
            if (existing is not null)
            {
                throw new ConflictException("En bruker med denne e-posten finnes allerede.");
            }
        }

        if (!user.IsActive && request.Role != user.Role)
        {
            throw new ConflictException("Inactive users cannot change role.");
        }

        if (user.Role == UserRole.Admin &&
            request.Role != UserRole.Admin &&
            await userRepository.CountActiveAdminsAsync(ct) == 1)
        {
            throw new ConflictException("The last active admin cannot be downgraded.");
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.EmailNormalized = normalizedEmail;
        user.Role = request.Role;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user, ct);
        return user;
    }
}
