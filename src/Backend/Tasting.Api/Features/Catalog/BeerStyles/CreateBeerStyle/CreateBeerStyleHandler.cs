using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;
using Tasting.Api.Infrastructure.Catalog;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed class CreateBeerStyleHandler(CatalogDbContext dbContext) : IRequestHandler<CreateBeerStyleCommand, BeerStyle>
{
    public async Task<BeerStyle> HandleAsync(CreateBeerStyleCommand request, CancellationToken ct = default)
    {
        var entity = new BeerStyle
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.BeerStyles.Add(entity);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }
}
