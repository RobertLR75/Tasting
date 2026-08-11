using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed class CreateUserHandler(IPersistenceService<User> users) : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> HandleAsync(CreateUserCommand request, CancellationToken ct = default)
    {
        if (request.Role == UserRole.Admin && !request.CallerIsAdmin)
        {
            throw new ForbiddenException("Kun eksisterende admin kan opprette nye admin-brukere.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToLowerInvariant();

        var existing = (await users.SearchAsync(new UserByNormalizedEmailSpecification(normalizedEmail), ct))
            .SingleOrDefault();
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true
        };

        await users.CreateAsync(user, ct);
        return user;
    }
}
