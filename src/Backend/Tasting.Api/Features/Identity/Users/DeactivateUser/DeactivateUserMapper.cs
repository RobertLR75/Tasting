using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Identity.Users.DeactivateUser;

public sealed class DeactivateUserMapper : BaseCommandMapper<DeactivateUserRequest, UserResponse, DeactivateUserCommand, User>
{
    public override DeactivateUserCommand ToCommand(DeactivateUserRequest req)
    {
        return new DeactivateUserCommand(req.Id);
    }

    public override Task<UserResponse> FromEntityAsync(User entity, CancellationToken ct = default)
    {
        return Task.FromResult(new UserResponse(
            entity.Id,
            entity.Email,
            entity.FirstName,
            entity.LastName,
            entity.IsActive,
            entity.Role,
            entity.CreatedAt,
            entity.UpdatedAt));
    }
}
