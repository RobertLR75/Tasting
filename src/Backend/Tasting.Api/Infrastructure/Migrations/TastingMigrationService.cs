using FluentMigrator.Runner;
using SharedLibrary.FluentMigration;

namespace Tasting.Api.Infrastructure.Migrations;

public sealed class TastingMigrationService : BaseFluentMigrationService
{
    protected override void ConfigureRunner(IMigrationRunnerBuilder runnerBuilder)
    {
        runnerBuilder.AddPostgres();
    }
}
