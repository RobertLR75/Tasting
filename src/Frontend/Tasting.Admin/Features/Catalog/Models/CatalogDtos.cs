namespace Tasting.Admin.Features.Catalog.Models;

public record BreweryDto(
    int Id,
    string Name,
    string Country
);

public record AddBreweryRequest(
    string Name,
    string Country
);

public record UpdateBreweryRequest(
    string Name,
    string Country
);

public record ListBreweriesResponse(
    IEnumerable<BreweryDto> Breweries,
    int Total
);

public record BeerDto(
    int Id,
    int BreweryId,
    string Name,
    string Style,
    decimal AlcoholPercentage
);

public record AddBeerRequest(
    string Name,
    string Style,
    decimal AlcoholPercentage
);

public record ListBeersResponse(
    IEnumerable<BeerDto> Beers,
    int Total
);
