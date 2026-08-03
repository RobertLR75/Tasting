using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.UnitTests.Identity;

internal static class UserTestData
{
    public static User RegularUser()
    {
        return new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "user@tasting.no",
            EmailNormalized = "user@tasting.no",
            FirstName = "Regular",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
