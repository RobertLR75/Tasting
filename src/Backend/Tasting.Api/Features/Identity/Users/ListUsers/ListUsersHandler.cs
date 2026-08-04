using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.ListUsers;

public sealed class ListUsersHandler(UsersDbContext dbContext) : IRequestHandler<ListUsersQuery, ListUsersResult>
{
    public async Task<ListUsersResult> HandleAsync(ListUsersQuery request, CancellationToken ct = default)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        var searchTerm = request.SearchTerm?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(u =>
                u.Email.ToLower().Contains(normalizedSearchTerm) ||
                u.FirstName.ToLower().Contains(normalizedSearchTerm) ||
                u.LastName.ToLower().Contains(normalizedSearchTerm));
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(ct);

        return new ListUsersResult(users);
    }
}
