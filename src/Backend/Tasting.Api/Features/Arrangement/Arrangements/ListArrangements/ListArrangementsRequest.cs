using Tasting.Api.Features.Arrangement.Domain;

namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed record ListArrangementsRequest(ArrangementStatus? Status = null);
