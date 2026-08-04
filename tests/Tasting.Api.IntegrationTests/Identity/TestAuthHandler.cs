using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tasting.Api.IntegrationTests.Identity;

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IntegrationTestAuth";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers[UserIdHeader].ToString();
        var role = Request.Headers[RoleHeader].ToString();

        if (!Guid.TryParse(userId, out _))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid X-Test-UserId header."));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Test-Role header."));
        }

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("role", role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName, "sub", "role");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
