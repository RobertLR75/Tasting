using SharedLibrary.FastEndpoints;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.BeerTypes;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Features.Identity.Users;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed class CreateBeerTypeEndpoint(IRequestHandler<CreateBeerTypeCommand, BeerType> handler)
    : BaseCommandEndpoint<CreateBeerTypeRequest, BeerTypeResponse, CreateBeerTypeCommand, BeerType, CreateBeerTypeMapper>(handler)
{
    public override void Configure()
    {
        Post("/beer-types");
        Description(d => d.WithTags("Beer Types"));
        Roles(UserRole.Admin.ToString());
    }
}
