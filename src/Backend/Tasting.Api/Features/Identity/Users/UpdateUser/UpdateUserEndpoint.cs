using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.UpdateUser;

public sealed class UpdateUserEndpoint(IRequestHandler<UpdateUserCommand, User> handler)
    : BaseCommandEndpoint<UpdateUserRequest, UserResponse, UpdateUserCommand, User, UpdateUserMapper>(handler)
{
    public override void Configure()
    {
        Put("/users/{id:guid}");
        Roles(UserRole.Admin.ToString());
    }

    protected override UpdateUserCommand ToCommand(UpdateUserRequest req)
    {
        return new UpdateUserCommand(req.Id, req.FirstName, req.LastName, req.Email, req.Role);
    }

    protected override async Task HandleResponseAsync(UserResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
