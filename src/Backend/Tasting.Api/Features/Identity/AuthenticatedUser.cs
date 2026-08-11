using System.Security.Claims;
using SharedLibrary.Services.Exceptions;

namespace Tasting.Api.Features.Identity;

public static class AuthenticatedUser
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new ForbiddenException("Unable to determine user identity from token.");
    }
}
