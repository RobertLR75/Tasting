using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Catalog.Breweries.ListBreweries;

public sealed record ListBreweriesQuery(bool IncludeInactive) : IRequest<ListBreweriesResponse>;
