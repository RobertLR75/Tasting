using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.UnitTests.Identity;

public sealed class ActiveUserMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AllowsAuthenticatedUserWithNameIdentifierClaim()
    {
        var userId = Guid.NewGuid();
        var nextWasCalled = false;
        var middleware = new ActiveUserMiddleware(_ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            ], "test"))
        };
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new User
            {
                Id = userId,
                Email = "admin@example.test",
                EmailNormalized = "admin@example.test",
                FirstName = "Ada",
                LastName = "Admin",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

        await middleware.InvokeAsync(context, userRepository);

        Assert.True(nextWasCalled);
    }
}
