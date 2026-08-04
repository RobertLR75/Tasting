using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed class CreateUserHandler(IUserRepository userRepository) : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> HandleAsync(CreateUserCommand request, CancellationToken ct = default)
    {
        if (request.Role == UserRole.Admin && !request.CallerIsAdmin)
        {
            throw new ForbiddenException("Kun eksisterende admin kan opprette nye admin-brukere.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        var existing = await userRepository.GetByEmailNormalizedAsync(normalizedEmail, ct);
        if (existing is not null)
        {
            throw new ConflictException("En bruker med denne e-posten finnes allerede.");
        }

        var user = new User
        {
            Email = email,
            EmailNormalized = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = request.Role,
            IsActive = true
        };

        await userRepository.CreateAsync(user, ct);
        return user;
    }
}
