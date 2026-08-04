using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerTypes.CreateBeerType;

public sealed class CreateBeerTypeHandler(CatalogDbContext dbContext) : IRequestHandler<CreateBeerTypeCommand, BeerType>
{
    public async Task<BeerType> HandleAsync(CreateBeerTypeCommand request, CancellationToken ct = default)
    {
        var entity = new BeerType
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.BeerTypes.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }
}
