using SharedLibrary.Interfaces;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Identity.Users.GetUser;

public sealed class GetUserHandler(IPersistenceService<User> users) : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> HandleAsync(GetUserQuery request, CancellationToken ct = default)
    {
        var user = await users.GetAsync(request.Id, ct);
        return user ?? throw new ServiceNotFoundException("Bruker ble ikke funnet.");
    }
}
