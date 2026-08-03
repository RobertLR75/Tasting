using SharedLibrary.FastEndpoints;

namespace Tasting.Api.Features.Catalog.Beers.ListBeers;

public sealed class ListBeersMapper : BaseQueryMapper<ListBeersRequest, ListBeersResponse, ListBeersQuery, ListBeersResult>
{
    public override ListBeersQuery ToQuery(ListBeersRequest req)
    {
        return new ListBeersQuery(req.IncludeInactive);
    }

    public override ListBeersResponse FromEntity(ListBeersResult entity)
    {
        return new ListBeersResponse
        {
            Beers = entity.Beers
                .Select(x => new ListBeersResponseItem
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

    public override Task<ListBeersResponse> FromEntityAsync(ListBeersResult entity, CancellationToken ct = default)
    {
        return Task.FromResult(FromEntity(entity));
    }
}
