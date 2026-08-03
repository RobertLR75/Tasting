using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.Beers.CreateBeer;

public sealed class CreateBeerEndpoint(IRequestHandler<CreateBeerCommand, Beer> handler)
    : BaseCommandEndpoint<CreateBeerRequest, CreateBeerResponse, CreateBeerCommand, Beer, CreateBeerMapper>(handler)
{
    public override void Configure()
    {
        Post("/beers");
        Roles("Admin");
    }
}
