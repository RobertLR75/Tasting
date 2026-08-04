using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Identity.Users.CreateUser;

public sealed class CreateUserMapper : BaseCommandMapper<CreateUserRequest, UserResponse, CreateUserCommand, User>
{
    public override CreateUserCommand ToCommand(CreateUserRequest req)
    {
        return new CreateUserCommand(req.Email, req.FirstName, req.LastName, req.Password, req.Role, false);
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
