using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;

public sealed record UpdateArrangementCommand(
    Guid ArrangementId,
    string Name,
    string? Description) : IRequest<Domain.Arrangement>;
