using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.GetUser;

public sealed record GetUserQuery(Guid Id) : IRequest<User>;
