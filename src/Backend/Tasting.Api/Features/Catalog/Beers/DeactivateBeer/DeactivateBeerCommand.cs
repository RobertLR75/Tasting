using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.DeactivateBeer;

public sealed record DeactivateBeerCommand(Guid Id) : IRequest<Beer>;
