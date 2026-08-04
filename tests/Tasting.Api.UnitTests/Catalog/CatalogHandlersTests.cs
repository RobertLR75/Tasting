using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedLibrary.Services.Exceptions;
using Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;
using Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;
using Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;
using Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;
using Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;
using Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;
using Tasting.Api.Features.Catalog.Beers.DeactivateBeer;
using Tasting.Api.Features.Catalog.Beers.GetBeer;
using Tasting.Api.Features.Catalog.Beers.UpdateBeer;
using Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;
using Tasting.Api.Features.Catalog.Breweries.GetBrewery;
using Tasting.Api.Features.Catalog.Breweries.ListBreweries;
using Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;
using Xunit;

namespace Tasting.Api.UnitTests.Catalog;

public sealed class CatalogHandlersTests
{
    [Fact]
    public async Task GetBrewery_Throws_WhenMissing()
    {
        await using var db = CreateDbContext();
        var sut = new GetBreweryHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.HandleAsync(new(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListBreweries_ExcludesInactive_WhenRequested()
    {
        await using var db = CreateDbContext();
        db.Breweries.AddRange(
            new Brewery { Id = Guid.NewGuid(), Name = "Active", IsActive = true, CreatedAt = DateTimeOffset.UtcNow },
            new Brewery { Id = Guid.NewGuid(), Name = "Inactive", IsActive = false, CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var sut = new ListBreweriesHandler(db);
        var result = await sut.HandleAsync(new(false), CancellationToken.None);

        Assert.Single(result.Breweries);
        Assert.Equal("Active", result.Breweries.Single().Name);
    }

    [Fact]
    public async Task UpdateBrewery_UpdatesFields()
    {
        await using var db = CreateDbContext();
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Before", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        db.Breweries.Add(brewery);
        await db.SaveChangesAsync();

        var sut = new UpdateBreweryHandler(db);
        var result = await sut.HandleAsync(new(brewery.Id, "After", false), CancellationToken.None);

        Assert.Equal("After", result.Name);
        Assert.False(result.IsActive);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task DeactivateBrewery_DeactivatesAssociatedBeers()
    {
        await using var db = CreateDbContext();
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer { Id = Guid.NewGuid(), BreweryId = brewery.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Beer", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        db.AddRange(brewery, style, type, beer);
        await db.SaveChangesAsync();

        var sut = new DeactivateBreweryHandler(db);
        await sut.HandleAsync(new(brewery.Id), CancellationToken.None);

        Assert.False((await db.Breweries.FindAsync(brewery.Id))!.IsActive);
        Assert.False((await db.Beers.FindAsync(beer.Id))!.IsActive);
    }

    [Fact]
    public async Task GetBeer_Throws_WhenMissing()
    {
        await using var db = CreateDbContext();
        var sut = new GetBeerHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.HandleAsync(new(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateBeer_ThrowsConflict_OnCaseInsensitiveDuplicate()
    {
        await using var db = CreateDbContext();
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer { Id = Guid.NewGuid(), BreweryId = brewery.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "One", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var other = new Beer { Id = Guid.NewGuid(), BreweryId = brewery.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Two", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        db.AddRange(style, type, brewery, beer, other);
        await db.SaveChangesAsync();

        var sut = new UpdateBeerHandler(db);

        await Assert.ThrowsAsync<ConflictException>(() => sut.HandleAsync(
            new UpdateBeerCommand(beer.Id, brewery.Id, style.Id, type.Id, "TWO", true),
            CancellationToken.None));
    }

    [Fact]
    public async Task DeactivateBeer_MarksBeerInactive()
    {
        await using var db = CreateDbContext();
        var style = new BeerStyle { Id = Guid.NewGuid(), Name = "IPA", CreatedAt = DateTimeOffset.UtcNow };
        var type = new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow };
        var brewery = new Brewery { Id = Guid.NewGuid(), Name = "Brew", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        var beer = new Beer { Id = Guid.NewGuid(), BreweryId = brewery.Id, BeerStyleId = style.Id, BeerTypeId = type.Id, Name = "Beer", IsActive = true, CreatedAt = DateTimeOffset.UtcNow };
        db.AddRange(style, type, brewery, beer);
        await db.SaveChangesAsync();

        var sut = new DeactivateBeerHandler(db);
        await sut.HandleAsync(new(beer.Id), CancellationToken.None);

        Assert.False((await db.Beers.FindAsync(beer.Id))!.IsActive);
    }

    [Fact]
    public async Task CreateBeerStyle_CreatesEntity()
    {
        await using var db = CreateDbContext();
        var sut = new CreateBeerStyleHandler(db);

        var result = await sut.HandleAsync(new("Sour"), CancellationToken.None);

        Assert.Equal("Sour", result.Name);
    }

    [Fact]
    public async Task GetBeerStyle_Throws_WhenMissing()
    {
        await using var db = CreateDbContext();
        var sut = new GetBeerStyleHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.HandleAsync(new(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListBeerStyles_ReturnsAlphabeticalOrder()
    {
        await using var db = CreateDbContext();
        db.BeerStyles.AddRange(
            new BeerStyle { Id = Guid.NewGuid(), Name = "Stout", CreatedAt = DateTimeOffset.UtcNow },
            new BeerStyle { Id = Guid.NewGuid(), Name = "Amber", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var sut = new ListBeerStylesHandler(db);
        var result = await sut.HandleAsync(new(), CancellationToken.None);

        Assert.Equal(["Amber", "Stout"], result.BeerStyles.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task CreateBeerType_CreatesEntity()
    {
        await using var db = CreateDbContext();
        var sut = new CreateBeerTypeHandler(db);

        var result = await sut.HandleAsync(new("Lager"), CancellationToken.None);

        Assert.Equal("Lager", result.Name);
    }

    [Fact]
    public async Task GetBeerType_Throws_WhenMissing()
    {
        await using var db = CreateDbContext();
        var sut = new GetBeerTypeHandler(db);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.HandleAsync(new(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ListBeerTypes_ReturnsAlphabeticalOrder()
    {
        await using var db = CreateDbContext();
        db.BeerTypes.AddRange(
            new BeerType { Id = Guid.NewGuid(), Name = "Stout", CreatedAt = DateTimeOffset.UtcNow },
            new BeerType { Id = Guid.NewGuid(), Name = "Ale", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var sut = new ListBeerTypesHandler(db);
        var result = await sut.HandleAsync(new(), CancellationToken.None);

        Assert.Equal(["Ale", "Stout"], result.BeerTypes.Select(x => x.Name).ToArray());
    }

    private static CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-unit-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new CatalogDbContext(options);
    }
}
