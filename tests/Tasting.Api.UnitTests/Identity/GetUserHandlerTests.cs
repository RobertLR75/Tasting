using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users.GetUser;

namespace Tasting.Api.UnitTests.Identity;

public sealed class GetUserHandlerTests
{
    [Fact]
    public async Task Should_throw_not_found_when_user_does_not_exist()
    {
        using var fixture = new HandlerTestFixture();
        var handler = new GetUserHandler(fixture.Repository);

        var act = async () => await handler.HandleAsync(new GetUserQuery(Guid.NewGuid()));

        await Assert.ThrowsAsync<ServiceNotFoundException>(act);
    }
}
