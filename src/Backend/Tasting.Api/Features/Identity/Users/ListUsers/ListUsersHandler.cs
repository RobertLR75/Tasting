using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersHandler(IPersistenceService<User> users) : IRequestHandler<ListUsersQuery, ListUsersResult>
{
    public async Task<ListUsersResult> HandleAsync(ListUsersQuery request, CancellationToken ct = default)
    {
        var result = await users.SearchAsync(new ListUsersSpecification(request.SearchTerm), ct);
        return new ListUsersResult(result);
    }
}
