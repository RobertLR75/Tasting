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
}
