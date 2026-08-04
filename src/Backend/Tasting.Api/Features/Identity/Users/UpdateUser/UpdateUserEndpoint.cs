using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.UpdateUser;

public sealed class UpdateUserEndpoint(IRequestHandler<UpdateUserCommand, User> handler)
    : BaseCommandEndpoint<UpdateUserRequest, UserResponse, UpdateUserCommand, User, UpdateUserMapper>(handler)
{
    public override void Configure()
    {
        Put("/users/{id:guid}");
        Description(d => d.WithTags("Users"));
        Roles(UserRole.Admin.ToString());
    }

protected override UpdateUserCommand ToCommand(UpdateUserRequest req)
{
    var id = Route<Guid>("id");
    return new UpdateUserCommand(id, req.FirstName, req.LastName, req.Email, req.Role);
}

    protected override async Task HandleResponseAsync(UserResponse response, CancellationToken ct)
    {
        await Send.ResponseAsync(response, StatusCodes.Status200OK, ct);
    }
}
