using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Security;

public interface ITokenService
{
    string GenerateToken(User user);
}
