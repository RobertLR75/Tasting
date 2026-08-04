using Microsoft.EntityFrameworkCore;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.ListUsers;

namespace Tasting.Api.UnitTests.Identity;

public sealed class ListUsersHandlerTests
{
    [Fact]
    public async Task Should_return_all_users_ordered_by_last_name_then_first_name()
    {
        using var fixture = new HandlerTestFixture();
        fixture.Context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "b@tasting.no",
            EmailNormalized = "b@tasting.no",
            FirstName = "B",
            LastName = "Zulu",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        fixture.Context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "a@tasting.no",
            EmailNormalized = "a@tasting.no",
            FirstName = "A",
            LastName = "Alpha",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new ListUsersHandler(fixture.Context);
        var result = await handler.HandleAsync(new ListUsersQuery());

        Assert.Equal(2, result.Users.Count);
        Assert.Equal("Alpha", result.Users.First().LastName);
        Assert.Equal("Zulu", result.Users.Last().LastName);
    }

    [Fact]
    public async Task Should_return_empty_list_when_no_users()
    {
        using var fixture = new HandlerTestFixture();

        var handler = new ListUsersHandler(fixture.Context);
        var result = await handler.HandleAsync(new ListUsersQuery());

        Assert.Empty(result.Users);
    }
}
