using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;

namespace Tasting.Api.Features.Catalog.BeerTypes.GetBeerType;

public sealed record GetBeerTypeQuery(Guid Id) : IRequest<BeerTypeResponse>;
