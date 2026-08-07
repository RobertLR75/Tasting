namespace Tasting.Admin.Features.Identity.Models;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Status
);

public record AddUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email
);

public record ChangeRoleRequest(
    string NewRole
);

public record ChangeStatusRequest(
    string NewStatus
);

public record ListUsersResponse(
    IEnumerable<UserDto> Users,
    int Total
);
