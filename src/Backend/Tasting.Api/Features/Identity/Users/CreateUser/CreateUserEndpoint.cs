using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed class CreateUserEndpoint(
    IRequestHandler<CreateUserCommand, User> handler)
    : BaseCommandEndpoint<CreateUserRequest, UserResponse, CreateUserCommand, User, CreateUserMapper>(handler)
{
    public override void Configure()
    {
        Post("/users");
        Description(d => d.WithTags("Users"));
        Roles(UserRole.Admin.ToString());
    }

    protected override CreateUserCommand ToCommand(CreateUserRequest req)
    {
        return new CreateUserCommand(
            req.Email,
            req.FirstName,
            req.LastName,
            req.Role,
            User.IsInRole(UserRole.Admin.ToString()));
    }
}
