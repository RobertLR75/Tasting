using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerTypes.ListBeerTypes;

public sealed class ListBeerTypesHandler(IPersistenceService<BeerType> types) : IRequestHandler<ListBeerTypesQuery, ListBeerTypesResponse>
{
    public async Task<ListBeerTypesResponse> HandleAsync(ListBeerTypesQuery request, CancellationToken ct = default)
    {
        var items = await types.SearchAsync(new AllBeerTypesSpecification(), ct);
        return new ListBeerTypesResponse
        {
            BeerTypes = items.Select(x => new BeerTypeSummaryResponse(x.Id, x.Name, x.CreatedAt, x.UpdatedAt)).ToList()
        };
    }
}
