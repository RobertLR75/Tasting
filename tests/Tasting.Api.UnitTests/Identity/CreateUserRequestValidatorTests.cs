using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Features.Identity.Users.CreateUser;

namespace Tasting.Api.UnitTests.Identity;

public sealed class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("robert.rodberget@gmail.com")]
    public async Task Should_pass_for_valid_email(string email)
    {
        var result = await _validator.ValidateAsync(ValidRequest(email: email));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("robert.rodberget")]
    public async Task Should_fail_for_invalid_email(string email)
    {
        var result = await _validator.ValidateAsync(ValidRequest(email: email));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.Email));
    }

    [Fact]
    public async Task Should_fail_when_email_exceeds_200_chars()
    {
        var result = await _validator.ValidateAsync(ValidRequest(email: new string('a', 196) + "@b.no"));
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_first_name_is_empty(string firstName)
    {
        var result = await _validator.ValidateAsync(ValidRequest(firstName: firstName));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_fail_when_last_name_is_empty(string lastName)
    {
        var result = await _validator.ValidateAsync(ValidRequest(lastName: lastName));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.LastName));
    }

    [Fact]
    public async Task Should_fail_when_password_is_too_short()
    {
        var result = await _validator.ValidateAsync(ValidRequest(password: "short"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Fact]
    public async Task Should_fail_when_password_is_empty()
    {
        var result = await _validator.ValidateAsync(ValidRequest(password: ""));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Should_pass_for_valid_request()
    {
        var result = await _validator.ValidateAsync(ValidRequest());
        Assert.True(result.IsValid);
    }

    private static CreateUserRequest ValidRequest(
        string email = "valid@tasting.no",
        string firstName = "Test",
        string lastName = "User",
        string password = "password123") => new()
    {
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        Password = password,
        Role = UserRole.User
    };
}
