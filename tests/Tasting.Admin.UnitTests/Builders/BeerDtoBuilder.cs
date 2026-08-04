using Tasting.Admin.Features.Catalog.Models;

namespace Tasting.Admin.UnitTests.Builders;

public class BeerDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _breweryId = Guid.NewGuid();
    private Guid _beerStyleId = Guid.NewGuid();
    private Guid _beerTypeId = Guid.NewGuid();
    private string _name = "Test Beer";
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _updatedAt = null;

    public BeerDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public BeerDtoBuilder WithBreweryId(Guid breweryId)
    {
        _breweryId = breweryId;
        return this;
    }

    public BeerDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BeerDtoBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public BeerDto Build()
    {
        return new BeerDto(_id, _breweryId, _beerStyleId, _beerTypeId, _name, _isActive, _createdAt, _updatedAt);
    }

    public static BeerDtoBuilder Default()
    {
        return new BeerDtoBuilder();
    }

    public static BeerDtoBuilder Inactive()
    {
        return new BeerDtoBuilder().WithIsActive(false);
    }
}
