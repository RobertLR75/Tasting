using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed class CreateBeerTypeHandler(IPersistenceService<BeerType> types) : IRequestHandler<CreateBeerTypeCommand, BeerType>
{
    public async Task<BeerType> HandleAsync(CreateBeerTypeCommand request, CancellationToken ct = default)
    {
        var entity = new BeerType
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await types.CreateAsync(entity, ct);
        return entity;
    }
}
