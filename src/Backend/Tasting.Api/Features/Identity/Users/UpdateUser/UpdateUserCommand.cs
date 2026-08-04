using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role) : IRequest<User>;
