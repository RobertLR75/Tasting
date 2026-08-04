using Microsoft.EntityFrameworkCore;
using Tasting.Api.Infrastructure.Identity;

namespace Tasting.Api.UnitTests.Identity;

internal sealed class HandlerTestFixture : IDisposable
{
    public UsersDbContext Context { get; }
    public IUserRepository Repository { get; }

    public HandlerTestFixture()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"tasting-api-unit-{Guid.NewGuid()}")
            .Options;

        Context = new UsersDbContext(options);
        Repository = new UserRepository(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
