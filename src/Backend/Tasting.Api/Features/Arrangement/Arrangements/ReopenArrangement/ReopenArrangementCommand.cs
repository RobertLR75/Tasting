using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.ReopenArrangement;

public sealed record ReopenArrangementCommand(Guid ArrangementId, uint RowVersion) : IRequest<Domain.Arrangement>;
