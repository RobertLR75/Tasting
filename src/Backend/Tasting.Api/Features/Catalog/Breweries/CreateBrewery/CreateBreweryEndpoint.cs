using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.Breweries.CreateBrewery;

public sealed class CreateBreweryEndpoint(IRequestHandler<CreateBreweryCommand, Brewery> handler)
    : BaseCommandEndpoint<CreateBreweryRequest, CreateBreweryResponse, CreateBreweryCommand, Brewery, CreateBreweryMapper>(handler)
{
    public override void Configure()
    {
        Post("/breweries");
        Description(d => d.WithTags("Breweries"));
        Roles(UserRole.Admin.ToString());
    }
}
