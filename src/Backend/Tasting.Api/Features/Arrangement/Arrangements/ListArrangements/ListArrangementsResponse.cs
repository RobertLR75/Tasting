namespace Tasting.Api.Features.Arrangement.Arrangements.ListArrangements;

public sealed record ListArrangementsResponse(IReadOnlyList<ArrangementResponse> Items);
