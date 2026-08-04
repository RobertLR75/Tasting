using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.UpdateBrewery;

public sealed record UpdateBreweryCommand(Guid Id, string Name, bool IsActive) : IRequest<Brewery>;
