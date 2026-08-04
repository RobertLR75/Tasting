using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed record ListArrangementsQuery(ArrangementStatus? Status) : IRequest<ListArrangementsResult>;
