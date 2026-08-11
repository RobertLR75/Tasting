using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.CompleteArrangement;

public sealed record CompleteArrangementCommand(Guid ArrangementId) : IRequest<Domain.Arrangement>;
