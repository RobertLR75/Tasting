using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Breweries;

namespace Tasting.Api.Features.Catalog.Breweries.GetBrewery;

public sealed record GetBreweryQuery(Guid Id) : IRequest<BreweryResponse>;
