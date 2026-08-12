using SharedLibrary.Interfaces;
using SharedLibrary.Services.Interfaces;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Features.Catalog.BeerStyles.CreateBeerStyle;

public sealed class CreateBeerStyleHandler(IPersistenceService<BeerStyle> styles) : IRequestHandler<CreateBeerStyleCommand, BeerStyle>
{
    public async Task<BeerStyle> HandleAsync(CreateBeerStyleCommand request, CancellationToken ct = default)
    {
        var entity = new BeerStyle
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await styles.CreateAsync(entity, ct);
        return entity;
    }
}
