using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.DeactivateUser;

namespace Tasting.Api.UnitTests.Identity;

public sealed class DeactivateUserHandlerTests
{
    [Fact]
    public async Task Should_deactivate_active_user()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();

        var handler = new DeactivateUserHandler(fixture.Repository);
        var result = await handler.HandleAsync(new DeactivateUserCommand(user.Id));

        Assert.False(result.IsActive);
        var persisted = await fixture.Repository.GetAsync(user.Id);
        Assert.NotNull(persisted);
        Assert.False(persisted!.IsActive);
    }

    [Fact]
    public async Task Should_throw_conflict_when_deactivating_last_active_admin()
    {
        using var fixture = new HandlerTestFixture();
        fixture.Context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@tasting.no",
            EmailNormalized = "admin@tasting.no",
            FirstName = "Only",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var adminId = fixture.Context.Users.Select(u => u.Id).Single();

        var handler = new DeactivateUserHandler(fixture.Repository);
        var act = async () => await handler.HandleAsync(new DeactivateUserCommand(adminId));

        await Assert.ThrowsAsync<ConflictException>(act);
    }
}
