using SharedLibrary.Services.Interfaces;

namespace Tasting.Api.Features.Rating.Results.GetResults;

public class GetResultsQuery : IRequest<GetResultsResponse>
{
    public Guid ArrangementId { get; set; }
}
