using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Arrangements.GetArrangement;

public sealed record GetArrangementQuery(Guid ArrangementId) : IRequest<Domain.Arrangement>;
