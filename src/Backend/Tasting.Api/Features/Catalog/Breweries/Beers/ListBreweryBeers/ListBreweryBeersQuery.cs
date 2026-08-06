using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed record ListBreweryBeersQuery(Guid BreweryId) : IRequest<ListBreweryBeersResult>;
