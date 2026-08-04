using Tasting.Admin.Features.Catalog.Models;

namespace Tasting.Admin.UnitTests.Builders;

public class BreweryDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Brewery";
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _updatedAt = null;

    public BreweryDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public BreweryDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BreweryDtoBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public BreweryDtoBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public BreweryDtoBuilder WithUpdatedAt(DateTimeOffset? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public BreweryDto Build()
    {
        return new BreweryDto(_id, _name, _isActive, _createdAt, _updatedAt);
    }

    public static BreweryDtoBuilder Default()
    {
        return new BreweryDtoBuilder();
    }

    public static BreweryDtoBuilder Inactive()
    {
        return new BreweryDtoBuilder().WithIsActive(false);
    }
}
