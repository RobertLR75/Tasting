using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Rating.Results.GetResults;

public class GetResultsEndpoint(IRequestHandler<GetResultsQuery, GetResultsResponse> handler)
    : Endpoint<GetResultsRequest, GetResultsResponse>
{
    public override void Configure()
    {
        Get("arrangements/{arrangementId}/results");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(d => d.WithTags("Arrangements"));
    }

    public override async Task HandleAsync(GetResultsRequest req, CancellationToken ct)
    {
        var query = new GetResultsQuery
        {
            ArrangementId = Route<Guid>("arrangementId")
        };

        Response = await handler.HandleAsync(query, ct);
        await Send.ResponseAsync(Response, 200, ct);
    }
}
