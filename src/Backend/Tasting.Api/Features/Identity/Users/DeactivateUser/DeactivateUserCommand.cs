using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : IRequest<User>;
