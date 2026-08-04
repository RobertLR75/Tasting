using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.DeactivateUser;

public sealed class DeactivateUserEndpoint(
    IRequestHandler<DeactivateUserCommand, User> handler)
    : BaseCommandEndpoint<DeactivateUserRequest, UserResponse, DeactivateUserCommand, User, DeactivateUserMapper>(handler)
{
    public override void Configure()
    {
        Patch("/users/{id:guid}/deactivate");
        Description(d => d.WithTags("Users"));
        Roles(UserRole.Admin.ToString());
    }

    protected override async Task HandleResponseAsync(UserResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
