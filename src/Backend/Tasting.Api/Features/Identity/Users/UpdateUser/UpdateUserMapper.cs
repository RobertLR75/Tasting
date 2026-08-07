using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Identity.Users.UpdateUser;

public sealed class UpdateUserMapper : BaseCommandMapper<UpdateUserRequest, UserResponse, UpdateUserCommand, User>
{
    public override UpdateUserCommand ToCommand(UpdateUserRequest req)
    {
        return new UpdateUserCommand(req.Id, req.FirstName, req.LastName, req.Email, req.Role);
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
