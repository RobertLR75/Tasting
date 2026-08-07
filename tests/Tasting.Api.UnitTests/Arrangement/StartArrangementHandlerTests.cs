using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;
using Tasting.Api.Features.Arrangement.Domain;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;
using Tasting.Api.Infrastructure.Arrangement;
using Tasting.Api.Infrastructure.Catalog;
using Tasting.Api.Infrastructure.Identity;
using Xunit;
using ArrangementEntity = Tasting.Api.Features.Arrangement.Domain.Arrangement;

namespace Tasting.Api.UnitTests.Arrangement;

public sealed class StartArrangementHandlerTests
{
    [Fact]
    public async Task HandleAsync_TransitionsToStarted_AndTakesSnapshots()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var (user, beer) = await SeedIdentityAndCatalogAsync(usersDb, catalogDb);
        var arrangement = await SeedArrangementWithParticipantAndBeerAsync(
            db, user.Id, beer.Id, ArrangementStatus.Active);

        var handler = new StartArrangementHandler(db, usersDb, catalogDb);

        var result = await handler.HandleAsync(
            new StartArrangementCommand(arrangement.Id, arrangement.RowVersion),
            CancellationToken.None);

        Assert.Equal(ArrangementStatus.Started, result.Status);
        Assert.Equal(1u, result.RowVersion);

        var participant = result.Participants.Single();
        Assert.Equal("Ola", participant.FirstNameSnapshot);
        Assert.Equal("Nordmann", participant.LastNameSnapshot);

        var arrangementBeer = result.Beers.Single();
        Assert.Equal("Test Beer", arrangementBeer.NameSnapshot);
        Assert.Equal("Test Brewery", arrangementBeer.BreweryNameSnapshot);
        Assert.Equal("IPA", arrangementBeer.BeerStyleSnapshot);
        Assert.Equal("Ale", arrangementBeer.BeerTypeSnapshot);
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenNotInActiveStatus()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Created);

        var handler = new StartArrangementHandler(db, usersDb, catalogDb);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new StartArrangementCommand(arrangement.Id, arrangement.RowVersion),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenRowVersionMismatch()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var arrangement = await SeedArrangementAsync(db, ArrangementStatus.Active);

        var handler = new StartArrangementHandler(db, usersDb, catalogDb);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(
            new StartArrangementCommand(arrangement.Id, RowVersion: 99u),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFound_WhenArrangementDoesNotExist()
    {
        await using var db = CreateArrangementDbContext();
        await using var usersDb = CreateUsersDbContext();
        await using var catalogDb = CreateCatalogDbContext();

        var handler = new StartArrangementHandler(db, usersDb, catalogDb);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => handler.HandleAsync(
            new StartArrangementCommand(Guid.NewGuid(), 0u),
            CancellationToken.None));
    }

    private static async Task<(User user, Beer beer)> SeedIdentityAndCatalogAsync(
        UsersDbContext usersDb, CatalogDbContext catalogDb)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ola@example.com",
            EmailNormalized = "ola@example.com",
            FirstName = "Ola",
            LastName = "Nordmann",
            IsActive = true,
            Role = UserRole.User,
            CreatedAt = DateTimeOffset.UtcNow
        };
        usersDb.Users.Add(user);
        await usersDb.SaveChangesAsync();

        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Test Brewery", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer
        {
            Id = Guid.NewGuid(),
            BreweryId = brewery.Id,
            BeerStyleId = style.Id,
            BeerTypeId = type.Id,
            Name = "Test Beer",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        catalogDb.AddRange(style, type, brewery, beer);
        await catalogDb.SaveChangesAsync();

        return (user, beer);
    }

    private static async Task<ArrangementEntity> SeedArrangementWithParticipantAndBeerAsync(
        ArrangementDbContext db, Guid userId, Guid beerId, ArrangementStatus status)
    {
        var arrangement = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Arrangement",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        arrangement.Participants.Add(new ArrangementParticipant
        {
            Id = Guid.NewGuid(),
            ArrangementId = arrangement.Id,
            UserId = userId,
            FirstNameSnapshot = string.Empty,
            LastNameSnapshot = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        });
        arrangement.Beers.Add(new ArrangementBeer
        {
            Id = Guid.NewGuid(),
            ArrangementId = arrangement.Id,
            BeerId = beerId,
            NameSnapshot = string.Empty,
            BreweryNameSnapshot = string.Empty,
            BeerStyleSnapshot = string.Empty,
            BeerTypeSnapshot = string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();
        return arrangement;
    }

    private static async Task<ArrangementEntity> SeedArrangementAsync(
        ArrangementDbContext db, ArrangementStatus status)
    {
        var arrangement = new ArrangementEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Status = status,
            RowVersion = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Arrangements.Add(arrangement);
        await db.SaveChangesAsync();
        return arrangement;
    }

    private static ArrangementDbContext CreateArrangementDbContext()
    {
        var options = new DbContextOptionsBuilder<ArrangementDbContext>()
            .UseInMemoryDatabase($"arrangement-unit-{Guid.NewGuid()}")
            .Options;
        return new ArrangementDbContext(options);
    }

    private static UsersDbContext CreateUsersDbContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"users-unit-{Guid.NewGuid()}")
            .Options;
        return new UsersDbContext(options);
    }

    private static CatalogDbContext CreateCatalogDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid()}")
            .Options;
        return new CatalogDbContext(options);
    }
}
