using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Arrangement.Participants.ListVisibleArrangements;

public sealed record ListVisibleArrangementsQuery(Guid UserId) : IRequest<ListVisibleArrangementsResponse>;

public sealed record ListVisibleArrangementsResponse(IReadOnlyList<VisibleArrangementResponse> Items);

public sealed record VisibleArrangementResponse(Guid Id, string Name, string? Description, bool Joined);
