using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.ActivateArrangement;

public sealed record ActivateArrangementCommand(
    Guid ArrangementId,
    uint RowVersion) : IRequest<Domain.Arrangement>;
