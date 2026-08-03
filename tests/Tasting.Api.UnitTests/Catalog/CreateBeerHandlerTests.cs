using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Catalog.Beers.CreateBeer;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;

namespace Tasting.Api.UnitTests.Catalog;

public sealed class CreateBeerHandlerTests
{
    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenBreweryIsInactive()
    {
        await using var dbContext = CreateDbContext();
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = false, CreatedAt = DateTimeOffset.UtcNow };
        dbContext.AddRange(style, type, brewery);
        await dbContext.SaveChangesAsync();

        var sut = new CreateBeerHandler(dbContext);

        await Assert.ThrowsAsync<ConflictException>(() => sut.HandleAsync(
            new CreateBeerCommand(brewery.Id, style.Id, type.Id, "Cloud", true),
            CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflict_WhenBeerNameAlreadyExistsCaseInsensitive()
    {
        await using var dbContext = CreateDbContext();
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var existingBeer = new Beer
        {
            Id = Guid.NewGuid(),
            BreweryId = brewery.Id,
            BeerStyleId = style.Id,
            BeerTypeId = type.Id,
            Name = "Pilsner",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.AddRange(style, type, brewery, existingBeer);
        await dbContext.SaveChangesAsync();

        var sut = new CreateBeerHandler(dbContext);

        await Assert.ThrowsAsync<ConflictException>(() => sut.HandleAsync(
            new CreateBeerCommand(brewery.Id, style.Id, type.Id, "PILSNER", true),
            CancellationToken.None));
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid()}")
            .Options;

        return new CatalogDbContext(options);
    }
}
