using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;
