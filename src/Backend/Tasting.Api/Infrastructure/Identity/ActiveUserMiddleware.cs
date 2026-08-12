using System.Security.Claims;
using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class ActiveUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IPersistenceService<User> users)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userIdClaim = context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ForbiddenException("Authenticated user mangler gyldig user id-claim.");
        }

        var user = await users.GetAsync(userId, context.RequestAborted);
        if (user is null || !user.IsActive)
        {
            throw new ForbiddenException("Brukeren er inaktiv.");
        }

        await next(context);
    }
}
