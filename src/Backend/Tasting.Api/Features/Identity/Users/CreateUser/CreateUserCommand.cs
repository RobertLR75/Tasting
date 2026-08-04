using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    UserRole Role,
    bool CallerIsAdmin) : IRequest<User>;
