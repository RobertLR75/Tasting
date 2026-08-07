using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Identity.Users.GetUser;

public sealed class GetUserMapper : BaseQueryMapper<GetUserRequest, UserResponse, GetUserQuery, User>
{
    public override GetUserQuery ToQuery(GetUserRequest req)
    {
        return new GetUserQuery(req.Id);
    }

    public override Task<UserResponse> FromEntityAsync(User entity, CancellationToken ct = default)
    {
        return Task.FromResult(new UserResponse(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.IsActive ? "Active" : "Inactive",
            entity.Role.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt));
    }
}
