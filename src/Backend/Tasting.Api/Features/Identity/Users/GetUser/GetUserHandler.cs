using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.Features.Identity.Users.GetUser;

public sealed class GetUserHandler(IUserRepository userRepository) : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> HandleAsync(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await userRepository.GetAsync(request.Id, ct);
        return user ?? throw new ServiceNotFoundException("Bruker ble ikke funnet.");
    }
}
