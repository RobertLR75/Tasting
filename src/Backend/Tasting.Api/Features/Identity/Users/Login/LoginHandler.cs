using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Infrastructure.Security;

namespace Tasting.Api.Features.Identity.Users.Login;

public sealed class LoginHandler(
    IPersistenceService<User> users,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(LoginCommand request, CancellationToken ct = default)
    {
        var user = (await users.SearchAsync(
                new UserByNormalizedEmailSpecification(request.Email.Trim().ToLowerInvariant()), ct))
            .SingleOrDefault();

        if (user is null ||
            !user.IsActive ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = tokenService.GenerateToken(user);

        return new LoginResponse(
            Token: token,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: user.Role.ToString()
        );
    }
}
