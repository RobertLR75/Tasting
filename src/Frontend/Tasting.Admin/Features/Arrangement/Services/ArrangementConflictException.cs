using System.Net;
using Tasting.Admin.Features.Arrangement.Models;

namespace Tasting.Admin.Features.Arrangement.Services;

public sealed class ArrangementConflictException(
    string message,
    ArrangementDto? freshArrangement)
    : HttpRequestException(message, null, HttpStatusCode.Conflict)
{
    public ArrangementDto? FreshArrangement { get; } = freshArrangement;
}
