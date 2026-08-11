using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Beers.RemoveBeer;

public sealed record RemoveBeerCommand(
    Guid ArrangementId,
    Guid BeerId) : IRequest<Domain.Arrangement>;
