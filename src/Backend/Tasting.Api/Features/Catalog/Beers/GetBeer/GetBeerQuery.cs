using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Beers;

namespace Tasting.Api.Features.Catalog.Beers.GetBeer;

public sealed record GetBeerQuery(Guid Id) : IRequest<BeerResponse>;
