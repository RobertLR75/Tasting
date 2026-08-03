using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedLibrary.Services.Interfaces;
using RatingEntity = Tasting.Api.Infrastructure.Rating.Entities.Rating;

namespace Tasting.Api.Features.Rating.Ratings.SubmitRating;

public class SubmitRatingEndpoint(IRequestHandler<SubmitRatingCommand, RatingEntity> handler)
    : Endpoint<SubmitRatingRequest, SubmitRatingResponse>
{
    public override void Configure()
    {
        Post("arrangements/{arrangementId}/ratings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d => d.WithTags("Rating"));
    }

    public override async Task HandleAsync(SubmitRatingRequest req, CancellationToken ct)
    {
        var participantId = GetParticipantId();

        var command = new SubmitRatingCommand
        {
            ArrangementId = Route<Guid>("arrangementId"),
            ParticipantId = participantId,
            BeerId = req.BeerId,
            Visibility = req.Visibility,
            Smell = req.Smell,
            Taste = req.Taste,
            Toast = req.Toast
        };

        var rating = await handler.HandleAsync(command, ct);

        Response = new SubmitRatingResponse
        {
            Id = rating.Id,
            ArrangementId = rating.ArrangementId,
            ParticipantId = rating.ParticipantId,
            BeerId = rating.BeerId,
            Visibility = rating.Visibility,
            Smell = rating.Smell,
            Taste = rating.Taste,
            Toast = rating.Toast,
            TotalRating = rating.TotalRating,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };

        var isNew = rating.UpdatedAt is null;
        await Send.ResponseAsync(Response, isNew ? 201 : 200, ct);
    }

    private Guid GetParticipantId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");

        if (sub is null || !Guid.TryParse(sub, out var participantId))
            throw new SharedLibrary.Services.Exceptions.ForbiddenException("Unable to determine participant identity from token.");

        return participantId;
    }
}
