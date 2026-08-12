using Ardalis.Specification;
using SharedLibrary.Interfaces;

namespace Tasting.Api.Features.Identity.Users;

public sealed class UserByNormalizedEmailSpecification : PersistenceSpecification<User>
{
    public UserByNormalizedEmailSpecification(string normalizedEmail)
        => Query.Where(user => user.EmailNormalized == normalizedEmail);
}

public sealed class ActiveAdminsSpecification : PersistenceSpecification<User>
{
    public ActiveAdminsSpecification()
    {
        Query.Where(user => user.IsActive && user.Role == UserRole.Admin);
        Query.Take(2);
    }
}

public sealed class ListUsersSpecification : PersistenceSpecification<User>
{
    public ListUsersSpecification(string? searchTerm)
    {
        var normalized = searchTerm?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            Query.Where(user =>
                user.Email.ToLower().Contains(normalized) ||
                user.FirstName.ToLower().Contains(normalized) ||
                user.LastName.ToLower().Contains(normalized));
        }

        Query.OrderBy(user => user.LastName).ThenBy(user => user.FirstName);
    }
}
