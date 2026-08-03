using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed record CreateBreweryCommand(string Name, bool IsActive) : IRequest<Brewery>;
