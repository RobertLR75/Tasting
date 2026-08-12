using Ardalis.Specification;
using SharedLibrary.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog;

public sealed class AllBreweriesSpecification : PersistenceSpecification<Brewery>
{
    public AllBreweriesSpecification(bool includeInactive)
    {
        if (!includeInactive)
        {
            Query.Where(x => x.IsActive);
        }

        Query.OrderBy(x => x.Name);
    }
}

public sealed class AllBeerStylesSpecification : PersistenceSpecification<BeerStyle>
{
    public AllBeerStylesSpecification() => Query.OrderBy(x => x.Name);
}

public sealed class AllBeerTypesSpecification : PersistenceSpecification<BeerType>
{
    public AllBeerTypesSpecification() => Query.OrderBy(x => x.Name);
}

public sealed class BeersWithCatalogSpecification : PersistenceSpecification<Beer>
{
    public BeersWithCatalogSpecification(bool includeInactive, Guid? breweryId = null)
    {
        if (!includeInactive)
        {
            Query.Where(x => x.IsActive);
        }

        if (breweryId.HasValue)
        {
            Query.Where(x => x.BreweryId == breweryId.Value);
        }

        Query
            .Include(x => x.Brewery)
            .Include(x => x.BeerStyle)
            .Include(x => x.BeerType)
            .OrderBy(x => x.Name);
    }
}

public sealed class BeerNameWithinBrewerySpecification : PersistenceSpecification<Beer>
{
    public BeerNameWithinBrewerySpecification(Guid breweryId, string name, Guid? excludedBeerId = null)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        Query.Where(x => x.BreweryId == breweryId && x.Name.ToLower() == normalizedName);

        if (excludedBeerId.HasValue)
        {
            Query.Where(x => x.Id != excludedBeerId.Value);
        }

        Query.Take(1);
    }
}

public sealed class ActiveBeersForBrewerySpecification : PersistenceSpecification<Beer>
{
    public ActiveBeersForBrewerySpecification(Guid breweryId)
        => Query.Where(x => x.BreweryId == breweryId && x.IsActive);
}

public sealed record BeerCatalogProjection(
    Guid Id,
    Guid BreweryId,
    Guid BeerStyleId,
    Guid BeerTypeId,
    string Name,
    bool IsActive);

public sealed class BeerCatalogProjectionSpecification : PersistenceSpecification<Beer, BeerCatalogProjection>
{
    public BeerCatalogProjectionSpecification()
        => Query
            .OrderBy(x => x.Name)
            .Select(x => new BeerCatalogProjection(
                x.Id,
                x.BreweryId,
                x.BeerStyleId,
                x.BeerTypeId,
                x.Name,
                x.IsActive));
}
