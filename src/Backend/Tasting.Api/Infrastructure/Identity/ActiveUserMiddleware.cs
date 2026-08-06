using System.Security.Claims;
using SharedLibrary.Services.Exceptions;

namespace Tasting.Api.Infrastructure.Identity;

public sealed class ActiveUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
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

        var user = await userRepository.GetAsync(userId, context.RequestAborted);
        if (user is null || !user.IsActive)
        {
            throw new ForbiddenException("Brukeren er inaktiv.");
        }

        await next(context);
    }
}
