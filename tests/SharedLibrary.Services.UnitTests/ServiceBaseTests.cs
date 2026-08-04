using NSubstitute;
using SharedLibrary.Interfaces;
using SharedLibrary.Services;
using SharedLibrary.Services.Exceptions;
using SharedLibrary.Services.Interfaces;

namespace SharedLibrary.Services.UnitTests;

public class ServiceBaseTests
{
    [Fact]
    public async Task CreateAsync_PersistsEntityReloadsItAndPublishesEvent()
    {
        var persistence = Substitute.For<IPersistenceService<TestEntity>>();
        var publisher = Substitute.For<IEventPublisher>();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        var created = new TestEntity { Id = entity.Id };
        persistence.CreateAsync(entity, Arg.Any<CancellationToken>()).Returns(entity.Id);
        persistence.GetAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(created);
        var sut = new TestService(persistence, publisher);

        var result = await sut.CreateAsync(entity, CancellationToken.None);

        Assert.Same(created, result);
        Assert.NotEqual(default, entity.CreatedAt);
        await publisher.Received(1).PublishAsync(Arg.Is<TestSharedEvent>(ev => ev.Id == entity.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenEntityDoesNotExist()
    {
        var persistence = Substitute.For<IPersistenceService<TestEntity>>();
        var publisher = Substitute.For<IEventPublisher>();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        persistence.GetAsync(entity.Id, Arg.Any<CancellationToken>()).Returns((TestEntity?)null);
        var sut = new TestService(persistence, publisher);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.UpdateAsync(entity, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenEntityDoesNotExist()
    {
        var persistence = Substitute.For<IPersistenceService<TestEntity>>();
        var publisher = Substitute.For<IEventPublisher>();
        persistence.GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TestEntity?)null);
        var sut = new TestService(persistence, publisher);

        await Assert.ThrowsAsync<ServiceNotFoundException>(() => sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task EventReceiver_ResolvesDeserializerAndInvokesCompletion()
    {
        var deserializer = Substitute.For<IMessageDeserializer<TestSharedEvent>>();
        deserializer.Deserialize("payload").Returns(new TestSharedEvent { Id = Guid.NewGuid() });
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IMessageDeserializer<TestSharedEvent>)).Returns(deserializer!);
        var sut = new EventReceiver(provider);
        TestSharedEvent? received = null;
        var completed = false;

        await sut.ReceiveEventAsync<TestSharedEvent>("payload", ev =>
        {
            received = ev;
            return Task.CompletedTask;
        }, () =>
        {
            completed = true;
            return Task.CompletedTask;
        });

        Assert.NotNull(received);
        Assert.True(completed);
    }

    private sealed class TestService(IPersistenceService<TestEntity> service, IEventPublisher publisher)
        : ServiceBase<TestEntity>(service, publisher)
    {
        protected override ISharedEvent? CreateCreatedEvent(TestEntity result) => new TestSharedEvent { Id = result.Id };
    }

    public sealed class TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public sealed class TestSharedEvent : ISharedEvent
    {
        public Guid Id { get; set; }
        public string SchemaVersion { get; init; } = "1.0";
        public DateTimeOffset OccurredAtUtc { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }
}
