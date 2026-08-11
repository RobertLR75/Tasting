using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.CancelArrangement;

public sealed record CancelArrangementCommand(Guid ArrangementId) : IRequest<Domain.Arrangement>;
