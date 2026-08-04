using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.StartArrangement;

public sealed record StartArrangementCommand(
    Guid ArrangementId,
    uint RowVersion) : IRequest<Domain.Arrangement>;
