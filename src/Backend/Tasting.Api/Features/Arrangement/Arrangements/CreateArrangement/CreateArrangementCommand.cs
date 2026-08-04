using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement.Arrangements.CreateArrangement;

public sealed record CreateArrangementCommand(
    string Name,
    string? Description) : IRequest<Domain.Arrangement>;
