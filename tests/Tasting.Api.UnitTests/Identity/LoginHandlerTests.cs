using NSubstitute;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users.Login;
using Tasting.Api.Infrastructure.Security;

namespace Tasting.Api.UnitTests.Identity;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Should_authenticate_active_participant_with_valid_credentials()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateToken(Arg.Any<Tasting.Api.Features.Identity.Users.User>()).Returns("participant-token");
        var handler = new LoginHandler(fixture.Repository, tokenService);

        var response = await handler.HandleAsync(new LoginCommand(user.Email, "password123"));

        Assert.Equal("participant-token", response.Token);
        Assert.Equal("User", response.Role);
    }

    [Fact]
    public async Task Should_reject_inactive_user_without_revealing_reason()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        user.IsActive = false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        var handler = new LoginHandler(fixture.Repository, Substitute.For<ITokenService>());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new LoginCommand(user.Email, "password123")));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task Should_reject_invalid_password_without_revealing_reason()
    {
        using var fixture = new HandlerTestFixture();
        var user = UserTestData.RegularUser();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        fixture.Context.Users.Add(user);
        await fixture.Context.SaveChangesAsync();
        var handler = new LoginHandler(fixture.Repository, Substitute.For<ITokenService>());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.HandleAsync(new LoginCommand(user.Email, "wrong-password")));

        Assert.Equal("Invalid email or password.", exception.Message);
    }
}
