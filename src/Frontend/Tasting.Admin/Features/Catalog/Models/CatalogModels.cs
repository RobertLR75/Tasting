namespace Tasting.Admin.Features.Catalog.Models;

public record BreweryDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record CreateBreweryRequest(
    string Name
);

public record UpdateBreweryRequest(
    string Name
);

public record ListBreweriesResponse(
    IEnumerable<BreweryDto> Breweries,
    int Total
);

public record BeerDto(
    Guid Id,
    Guid BreweryId,
    Guid BeerStyleId,
    Guid BeerTypeId,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record CreateBeerRequest(
    Guid BreweryId,
    Guid BeerStyleId,
    Guid BeerTypeId,
    string Name
);

public record UpdateBeerRequest(
    string Name
);

public record ListBeersResponse(
    IEnumerable<BeerDto> Beers,
    int Total
);

public record BeerStyleDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record ListBeerStylesResponse(
    IEnumerable<BeerStyleDto> BeerStyles
);

public record BeerTypeDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record ListBeerTypesResponse(
    IEnumerable<BeerTypeDto> BeerTypes
);
