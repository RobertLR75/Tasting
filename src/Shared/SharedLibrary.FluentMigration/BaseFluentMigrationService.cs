using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace SharedLibrary.FluentMigration;

public abstract class BaseFluentMigrationService
{
    protected virtual IReadOnlyCollection<Assembly> MigrationAssemblies => [GetType().Assembly];

    protected abstract void ConfigureRunner(IMigrationRunnerBuilder runnerBuilder);

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public ServiceProvider BuildServiceProvider(string connectionString, params string[] tags)
    {
        var services = new ServiceCollection();

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder =>
            {
                ConfigureRunner(runnerBuilder);
                runnerBuilder.WithGlobalConnectionString(connectionString);

                foreach (var assembly in MigrationAssemblies)
                {
                    runnerBuilder.ScanIn(assembly).For.Migrations();
                }
            });

        if (tags != null && tags.Length > 0)
        {
            services.Configure<RunnerOptions>(opt => opt.Tags = tags);
        }

        ConfigureServices(services);

        return services.BuildServiceProvider();
    }

    public virtual void MigrateUp(string connectionString, params string[] tags)
    {
        using var provider = BuildServiceProvider(connectionString, tags);
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        ExecuteMigrateUp(runner);
    }

    public void MigrateUp<TMigration>(string connectionString) where TMigration : class
    {
        MigrateUp(connectionString, typeof(TMigration));
    }

    public void MigrateUp(string connectionString, Type migrationType)
    {
        ArgumentNullException.ThrowIfNull(migrationType);

        using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        var targetVersion = ResolveTargetVersion(migrationType);
        ExecuteMigrateUp(runner, targetVersion);
    }

    public void MigrateUp(string connectionString, object migration)
    {
        ArgumentNullException.ThrowIfNull(migration);

        using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        var targetVersion = ResolveTargetVersion(migration);
        ExecuteMigrateUp(runner, targetVersion);
    }

    public void MigrateDown(string connectionString, long version)
    {
        using var provider = BuildServiceProvider(connectionString);
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateDown(version);
    }

    protected virtual long ResolveTargetVersion(Type migrationType)
    {
        ArgumentNullException.ThrowIfNull(migrationType);

        var attribute = migrationType.GetCustomAttribute<MigrationAttribute>();

        if (attribute is null)
        {
            throw new InvalidOperationException($"Migration type '{migrationType.FullName}' is missing {nameof(MigrationAttribute)}.");
        }

        return attribute.Version;
    }

    protected virtual long ResolveTargetVersion(object migration)
    {
        ArgumentNullException.ThrowIfNull(migration);

        return ResolveTargetVersion(migration.GetType());
    }

    protected virtual void ExecuteMigrateUp(IMigrationRunner runner, long? targetVersion = null)
    {
        ArgumentNullException.ThrowIfNull(runner);

        if (targetVersion.HasValue)
        {
            runner.MigrateUp(targetVersion.Value);
            return;
        }

        runner.MigrateUp();
    }
}
