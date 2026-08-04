using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.DeactivateBrewery;

public sealed record DeactivateBreweryCommand(Guid Id) : IRequest<Brewery>;
