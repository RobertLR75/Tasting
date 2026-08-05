using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.CreateUser;

namespace Tasting.Api.UnitTests.Identity;

public sealed class CreateUserHandlerTests
{
    [Fact]
    public async Task Should_throw_conflict_when_email_exists_case_insensitive()
    {
        using var fixture = new HandlerTestFixture();
        fixture.Context.Users.Add(UserTestData.RegularUser());
        await fixture.Context.SaveChangesAsync();

        var handler = new CreateUserHandler(fixture.Repository);
        var act = async () => await handler.HandleAsync(
            new CreateUserCommand("USER@TASTING.NO", "Another", "User", "password123", UserRole.User, false));

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task Should_throw_forbidden_when_non_admin_creates_admin()
    {
        using var fixture = new HandlerTestFixture();
        var handler = new CreateUserHandler(fixture.Repository);

        var act = async () => await handler.HandleAsync(
            new CreateUserCommand("admin2@tasting.no", "New", "Admin", "password123", UserRole.Admin, false));

        await Assert.ThrowsAsync<ForbiddenException>(act);
    }

    [Fact]
    public async Task Should_create_user_with_normalized_email()
    {
        using var fixture = new HandlerTestFixture();
        var handler = new CreateUserHandler(fixture.Repository);

        var user = await handler.HandleAsync(
            new CreateUserCommand("New.User@Tasting.NO", "New", "User", "password123", UserRole.User, false));

        Assert.Equal("new.user@tasting.no", user.EmailNormalized);
        Assert.Equal("New.User@Tasting.NO", user.Email);
    }

    [Fact]
    public async Task Should_allow_admin_to_create_admin()
    {
        using var fixture = new HandlerTestFixture();
        var handler = new CreateUserHandler(fixture.Repository);

        var user = await handler.HandleAsync(
            new CreateUserCommand("admin2@tasting.no", "New", "Admin", "password123", UserRole.Admin, true));

        Assert.Equal(UserRole.Admin, user.Role);
    }
}
