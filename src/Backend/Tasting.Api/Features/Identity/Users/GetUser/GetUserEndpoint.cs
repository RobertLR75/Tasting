using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.GetUser;

public sealed class GetUserEndpoint(
    IRequestHandler<GetUserQuery, User> handler)
    : BaseQueryEndpoint<GetUserRequest, UserResponse, GetUserQuery, User, GetUserMapper>(handler)
{
    public override void Configure()
    {
        Get("/users/{id:guid}");
        Roles(UserRole.Admin.ToString(), UserRole.User.ToString());
    }
}
