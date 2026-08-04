using FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SharedLibrary.FluentMigration;

namespace SharedLibrary.FluentMigration.UnitTests;

public class BaseFluentMigrationServiceTests
{
    [Fact]
    public void ResolveTargetVersion_Throws_WhenMigrationAttributeIsMissing()
    {
        var sut = new TestMigrationService();

        var exception = Assert.Throws<InvalidOperationException>(() => sut.ResolveVersion(typeof(MissingAttributeMigration)));

        Assert.Contains("missing MigrationAttribute", exception.Message);
    }

    [Fact]
    public void ExecuteMigrateUp_UsesTargetVersion_WhenProvided()
    {
        var sut = new TestMigrationService();
        var runner = Substitute.For<IMigrationRunner>();

        sut.ExecuteUp(runner, 42);

        runner.Received(1).MigrateUp(42);
        runner.DidNotReceive().MigrateUp();
    }

    [Fact]
    public void BuildServiceProvider_RegistersRunner()
    {
        var sut = new TestMigrationService();

        using var provider = sut.BuildServiceProvider("Host=localhost");

        Assert.NotNull(provider.GetService<IMigrationRunner>());
    }

    private sealed class TestMigrationService : BaseFluentMigrationService
    {
        protected override void ConfigureRunner(IMigrationRunnerBuilder runnerBuilder)
        {
            runnerBuilder.AddSQLite();
        }

        public long ResolveVersion(Type migrationType) => ResolveTargetVersion(migrationType);

        public void ExecuteUp(IMigrationRunner runner, long? version = null) => ExecuteMigrateUp(runner, version);
    }

    private sealed class MissingAttributeMigration;
}
