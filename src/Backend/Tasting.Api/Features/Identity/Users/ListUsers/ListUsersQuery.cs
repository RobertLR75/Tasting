using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed record ListUsersQuery(string? SearchTerm = null) : IRequest<ListUsersResult>;
