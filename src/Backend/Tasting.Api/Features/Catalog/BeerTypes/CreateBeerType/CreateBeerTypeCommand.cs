using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed record CreateBeerTypeCommand(string Name) : IRequest<BeerType>;
