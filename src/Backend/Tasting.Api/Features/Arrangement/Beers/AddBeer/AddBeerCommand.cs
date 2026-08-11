using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Beers.AddBeer;

public sealed record AddBeerCommand(
    Guid ArrangementId,
    Guid BeerId) : IRequest<Domain.Arrangement>;
