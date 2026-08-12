using System.Data.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedLibrary.Interfaces;
using SharedLibrary.PostgreSql.Dapper;
using SharedLibrary.PostgreSql.EntityFramework;
using Tasting.Api.Features.Catalog;
using Tasting.Api.Features.Catalog.Domain;

namespace Tasting.Api.Infrastructure.Catalog;

public sealed class EfCatalogStorage<T>(CatalogDbContext context)
    : EntityFrameworkPostgresSqlStorageBase<T>(context)
    where T : class, IEntity;

internal abstract class DapperCatalogStorage<T>(DbConnection connection, string tableName)
    : PostgresSqlDapperStorageBase<T>(connection)
    where T : class, IEntity
{
    protected override string TableName => tableName;

    protected override string MapPropertyToColumn(string propertyName) => propertyName switch
    {
        nameof(IEntity.Id) => "id",
        nameof(IEntity.CreatedAt) => "created_at_utc",
        nameof(IEntity.UpdatedAt) => "updated_at_utc",
        nameof(Brewery.Name) => "name",
        nameof(Brewery.IsActive) => "is_active",
        nameof(Beer.BreweryId) => "brewery_id",
        nameof(Beer.BeerStyleId) => "beer_style_id",
        nameof(Beer.BeerTypeId) => "beer_type_id",
        _ => propertyName
    };
}

internal sealed class DapperBreweryStorage(DbConnection connection)
    : DapperCatalogStorage<Brewery>(connection, "breweries");

internal sealed class DapperBeerStyleStorage(DbConnection connection)
    : DapperCatalogStorage<BeerStyle>(connection, "beer_styles");

internal sealed class DapperBeerTypeStorage(DbConnection connection)
    : DapperCatalogStorage<BeerType>(connection, "beer_types");

internal sealed class DapperBeerStorage(DbConnection connection)
    : DapperCatalogStorage<Beer>(connection, "beers")
{
    protected override IReadOnlyCollection<DapperRelationship> Relationships =>
    [
        DapperRelationship.Reference<Beer, Brewery>(
            nameof(Beer.Brewery), "breweries", "brewery_id", "id",
            (beer, brewery) => beer.Brewery = brewery!, MapPropertyToColumn),
        DapperRelationship.Reference<Beer, BeerStyle>(
            nameof(Beer.BeerStyle), "beer_styles", "beer_style_id", "id",
            (beer, style) => beer.BeerStyle = style!, MapPropertyToColumn),
        DapperRelationship.Reference<Beer, BeerType>(
            nameof(Beer.BeerType), "beer_types", "beer_type_id", "id",
            (beer, type) => beer.BeerType = type!, MapPropertyToColumn)
    ];
}

public sealed class EfCatalogDeactivationService(CatalogDbContext context) : ICatalogDeactivationService
{
    public async Task SaveDeactivationAsync(
        Brewery brewery,
        IReadOnlyCollection<Beer> beers,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.Breweries.Update(brewery);
        context.Beers.UpdateRange(beers);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}

internal sealed class DapperCatalogDeactivationService(NpgsqlConnection connection) : ICatalogDeactivationService
{
    public async Task SaveDeactivationAsync(
        Brewery brewery,
        IReadOnlyCollection<Beer> beers,
        CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE breweries SET is_active = @IsActive, updated_at_utc = @UpdatedAt WHERE id = @Id;",
            brewery, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE beers SET is_active = @IsActive, updated_at_utc = @UpdatedAt WHERE id = @Id;",
            beers, transaction, cancellationToken: cancellationToken));
        await transaction.CommitAsync(cancellationToken);
    }
}
