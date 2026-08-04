namespace Tasting.Api.Features.Arrangement.Arrangements.UpdateArrangement;

public sealed record UpdateArrangementRequest(string Name, string? Description, uint RowVersion);
