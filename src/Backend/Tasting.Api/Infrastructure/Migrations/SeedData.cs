using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Infrastructure.Migrations;

/// <summary>
/// Seed script to create initial test users for development/testing.
/// Run this after migrations to populate the database with test data.
/// </summary>
public static class SeedData
{
    public static void SeedTestUsers(IUserRepository userRepository)
    {
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            EmailNormalized = "admin@example.com".ToLowerInvariant(),
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123"), // Change in production!
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            EmailNormalized = "user@example.com".ToLowerInvariant(),
            FirstName = "Regular",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user@123"),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            EmailNormalized = "inactive@example.com".ToLowerInvariant(),
            FirstName = "Inactive",
            LastName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("inactive@123"),
            Role = UserRole.User,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };

        // Would be called from a seeding service after migrations run
        // await userRepository.CreateAsync(adminUser, CancellationToken.None);
        // await userRepository.CreateAsync(regularUser, CancellationToken.None);
        // await userRepository.CreateAsync(inactiveUser, CancellationToken.None);
    }
}
