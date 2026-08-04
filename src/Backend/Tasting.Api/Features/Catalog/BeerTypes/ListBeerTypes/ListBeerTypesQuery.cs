using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed record ListBeerTypesQuery : IRequest<ListBeerTypesResponse>;
