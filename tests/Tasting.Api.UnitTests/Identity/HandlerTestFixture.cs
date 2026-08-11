using SharedLibrary.Interfaces;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.UnitTests.Identity;

internal sealed class HandlerTestFixture : IDisposable
{
    public ProviderNeutralUserStore Context { get; } = new();
    public IPersistenceService<User> Repository => Context;

    public void Dispose()
    {
    }
}

internal sealed class ProviderNeutralUserStore : IPersistenceService<User>
{
    public UserCollection Users { get; } = [];
    public NoOpChangeTracker ChangeTracker { get; } = new();

    public Task SaveChangesAsync() => Task.CompletedTask;

    public Task<Guid> CreateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = entity.CreatedAt == default ? DateTimeOffset.UtcNow : entity.CreatedAt;
        Users.Add(entity);
        return Task.FromResult(entity.Id);
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Users.RemoveAll(user => user.Id == id);
        return Task.CompletedTask;
    }

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(Users.SingleOrDefault(user => user.Id == id));

    public Task<List<User>> SearchAsync(
        IPersistenceSpecification<User> specification,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<User> result = specification switch
        {
            UserByNormalizedEmailSpecification byEmail => Users.Where(user =>
                byEmail.WhereExpressions.Single().Filter.Compile()(user)),
            ActiveAdminsSpecification => Users.Where(user => user.IsActive && user.Role == UserRole.Admin),
            ListUsersSpecification list => ApplyListSpecification(list),
            _ => throw new NotSupportedException($"Unsupported test specification {specification.GetType().Name}.")
        };

        return Task.FromResult(result.ToList());
    }

    public async Task<User> GetAsync(
        IPersistenceSpecification<User> specification,
        CancellationToken cancellationToken = default)
        => (await SearchAsync(specification, cancellationToken)).First();

    public Task<List<TResult>> SearchAsync<TResult>(
        IPersistenceSpecification<User, TResult> specification,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    private IEnumerable<User> ApplyListSpecification(ListUsersSpecification specification)
    {
        var result = Users.AsEnumerable();
        var predicate = specification.WhereExpressions.SingleOrDefault()?.Filter.Compile();
        if (predicate is not null)
        {
            result = result.Where(predicate);
        }

        return result.OrderBy(user => user.LastName).ThenBy(user => user.FirstName);
    }
}

internal sealed class UserCollection : List<User>
{
    public void AddRange(params User[] users) => base.AddRange(users);
}

internal sealed class NoOpChangeTracker
{
    public void Clear()
    {
    }
}
