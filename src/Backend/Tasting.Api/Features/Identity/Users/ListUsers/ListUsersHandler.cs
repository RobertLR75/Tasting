using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersHandler(UsersDbContext dbContext) : IRequestHandler<ListUsersQuery, ListUsersResult>
{
    public async Task<ListUsersResult> HandleAsync(ListUsersQuery request, CancellationToken ct = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        return new ListUsersResult(users);
    }
}
