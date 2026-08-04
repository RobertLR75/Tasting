using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.UpdateUser;

namespace Tasting.Api.UnitTests.Identity;

public sealed class UpdateUserHandlerTests
{
    [Fact]
    public async Task Should_update_user_fields()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = new UpdateUserHandler(fixture.Repository);
        var result = await handler.HandleAsync(
            new UpdateUserCommand(user.Id, "Updated", "Name", "updated@tasting.no", UserRole.Admin));

        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal("updated@tasting.no", result.Email);
        Assert.Equal(UserRole.Admin, result.Role);
    }

    [Fact]
    public async Task Should_throw_not_found_when_user_does_not_exist()
    {
        using var fixture = new HandlerTestFixture();
        var handler = new UpdateUserHandler(fixture.Repository);

        var act = async () => await handler.HandleAsync(
            new UpdateUserCommand(Guid.NewGuid(), "A", "B", "x@tasting.no", UserRole.User));

        await Assert.ThrowsAsync<ServiceNotFoundException>(act);
    }

    [Fact]
    public async Task Should_throw_conflict_when_new_email_exists_on_another_user()
    {
        using var fixture = new HandlerTestFixture();
        var userA = UserTestData.RegularUser();
        var userB = new User
        {
            Id = Guid.NewGuid(),
            Email = "other@tasting.no",
            EmailNormalized = "other@tasting.no",
            FirstName = "Other",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        fixture.Context.Users.AddRange(userA, userB);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = new UpdateUserHandler(fixture.Repository);
        var act = async () => await handler.HandleAsync(
            new UpdateUserCommand(userA.Id, "Regular", "User", "OTHER@TASTING.NO", UserRole.User));

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task Should_allow_update_with_same_email_case_insensitive()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = new UpdateUserHandler(fixture.Repository);
        var result = await handler.HandleAsync(
            new UpdateUserCommand(user.Id, "Regular", "User", "USER@TASTING.NO", UserRole.User));

        Assert.Equal("USER@TASTING.NO".Trim(), result.Email);
    }
}
