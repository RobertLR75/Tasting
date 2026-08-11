using Tasting.Admin.Features.Arrangement.Models;

namespace Tasting.Admin.UnitTests.Builders;

public class ArrangementDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Arrangement";
    private string? _description = null;
    private ArrangementStatus _status = ArrangementStatus.Created;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _updatedAt = null;

    public ArrangementDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ArrangementDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ArrangementDtoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ArrangementDtoBuilder WithStatus(ArrangementStatus status)
    {
        _status = status;
        return this;
    }

    public ArrangementDtoBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ArrangementDtoBuilder WithUpdatedAt(DateTimeOffset? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public ArrangementDto Build()
    {
        return new ArrangementDto(_id, _name, _description, _status, _createdAt, _updatedAt, [], []);
    }

    public static ArrangementDtoBuilder Default()
    {
        return new ArrangementDtoBuilder();
    }

    public static ArrangementDtoBuilder Started()
    {
        return new ArrangementDtoBuilder().WithStatus(ArrangementStatus.Started);
    }

    public static ArrangementDtoBuilder Active()
    {
        return new ArrangementDtoBuilder().WithStatus(ArrangementStatus.Active);
    }

    public static ArrangementDtoBuilder Completed()
    {
        return new ArrangementDtoBuilder().WithStatus(ArrangementStatus.Completed);
    }

    public static ArrangementDtoBuilder Canceled()
    {
        return new ArrangementDtoBuilder().WithStatus(ArrangementStatus.Canceled);
    }
}
