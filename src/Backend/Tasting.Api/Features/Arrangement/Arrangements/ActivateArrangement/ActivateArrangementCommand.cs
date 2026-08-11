using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.ActivateArrangement;

public sealed record ActivateArrangementCommand(
    Guid ArrangementId) : IRequest<Domain.Arrangement>;
