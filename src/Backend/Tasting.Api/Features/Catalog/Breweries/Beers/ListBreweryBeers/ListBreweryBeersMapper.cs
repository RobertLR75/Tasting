using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Catalog.Breweries.Beers.ListBreweryBeers;

public sealed class ListBreweryBeersMapper : BaseQueryMapper<ListBreweryBeersRequest, ListBreweryBeersResponse, ListBreweryBeersQuery, ListBreweryBeersResult>
{
    public override ListBreweryBeersQuery ToQuery(ListBreweryBeersRequest req)
    {
        return new ListBreweryBeersQuery(req.BreweryId);
    }

    public override ListBreweryBeersResponse FromEntity(ListBreweryBeersResult entity)
    {
        return new ListBreweryBeersResponse
        {
            Beers = entity.Beers
                .Select(x => new ListBreweryBeersResponseItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    BreweryId = x.BreweryId,
                    BreweryName = x.BreweryName,
                    BeerStyleId = x.BeerStyleId,
                    BeerStyleName = x.BeerStyleName,
                    BeerTypeId = x.BeerTypeId,
                    BeerTypeName = x.BeerTypeName
                })
                .ToList()
        };
    }

    public override Task<ListBreweryBeersResponse> FromEntityAsync(ListBreweryBeersResult entity, CancellationToken ct = default)
    {
        return Task.FromResult(FromEntity(entity));
    }
}
