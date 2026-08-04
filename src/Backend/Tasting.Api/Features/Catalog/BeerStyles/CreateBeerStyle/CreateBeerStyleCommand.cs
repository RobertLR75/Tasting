using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed record CreateBeerStyleCommand(string Name) : IRequest<BeerStyle>;
