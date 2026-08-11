using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;

public sealed record StartArrangementCommand(
    Guid ArrangementId) : IRequest<Domain.Arrangement>;
