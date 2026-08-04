using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration config)
    {
        _secretKey = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured");
        _issuer = config["Jwt:Issuer"] ?? "TastingApi";
        _audience = config["Jwt:Audience"] ?? "TastingAdmin";
        _expirationMinutes = int.Parse(config["Jwt:ExpirationMinutes"] ?? "480"); // 8 hours
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
