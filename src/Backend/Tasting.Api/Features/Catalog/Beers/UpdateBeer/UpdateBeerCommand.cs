using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.UpdateBeer;

public sealed record UpdateBeerCommand(
    Guid Id,
    Guid BreweryId,
    Guid BeerStyleId,
    Guid BeerTypeId,
    string Name,
    bool IsActive) : IRequest<Beer>;
