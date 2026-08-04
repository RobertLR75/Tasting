using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;

namespace Tasting.Api.Features.Catalog.BeerStyles.GetBeerStyle;

public sealed record GetBeerStyleQuery(Guid Id) : IRequest<BeerStyleResponse>;
