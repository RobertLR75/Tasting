using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    bool CallerIsAdmin) : IRequest<User>;
