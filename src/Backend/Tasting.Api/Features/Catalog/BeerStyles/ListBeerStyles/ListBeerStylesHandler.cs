using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerStyles;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerStyles.ListBeerStyles;

public sealed class ListBeerStylesHandler(IPersistenceService<BeerStyle> styles) : IRequestHandler<ListBeerStylesQuery, ListBeerStylesResponse>
{
    public async Task<ListBeerStylesResponse> HandleAsync(ListBeerStylesQuery request, CancellationToken ct = default)
    {
        var items = await styles.SearchAsync(new AllBeerStylesSpecification(), ct);
        return new ListBeerStylesResponse
        {
            BeerStyles = items.Select(x => new BeerStyleSummaryResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt)).ToList()
        };
    }
}
