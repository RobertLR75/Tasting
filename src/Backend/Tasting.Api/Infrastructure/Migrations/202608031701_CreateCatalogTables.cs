using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202608031701)]
public sealed class CreateCatalogTablesMigration : Migration
{
    public override void Up()
    {
        Create.Table("breweries")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.Table("beer_styles")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.Table("beer_types")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.Table("beers")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("brewery_id").AsGuid().NotNullable()
            .WithColumn("beer_style_id").AsGuid().NotNullable()
            .WithColumn("beer_type_id").AsGuid().NotNullable()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.ForeignKey("fk_beers_brewery_id_breweries_id")
            .FromTable("beers").ForeignColumn("brewery_id")
            .ToTable("breweries").PrimaryColumn("id");

        Create.ForeignKey("fk_beers_beer_style_id_beer_styles_id")
            .FromTable("beers").ForeignColumn("beer_style_id")
            .ToTable("beer_styles").PrimaryColumn("id");

        Create.ForeignKey("fk_beers_beer_type_id_beer_types_id")
            .FromTable("beers").ForeignColumn("beer_type_id")
            .ToTable("beer_types").PrimaryColumn("id");

        Create.Index("ix_beers_brewery_id")
            .OnTable("beers")
            .OnColumn("brewery_id").Ascending();

        Create.Index("ix_beers_beer_style_id")
            .OnTable("beers")
            .OnColumn("beer_style_id").Ascending();

        Create.Index("ix_beers_beer_type_id")
            .OnTable("beers")
            .OnColumn("beer_type_id").Ascending();

        Execute.Sql("""
                    CREATE UNIQUE INDEX ux_beers_brewery_name_ci
                    ON beers (brewery_id, lower(name));
                    """);
    }

    public override void Down()
    {
        Delete.Index("ux_beers_brewery_name_ci").OnTable("beers");
        Delete.Table("beers");
        Delete.Table("beer_types");
        Delete.Table("beer_styles");
        Delete.Table("breweries");
    }
}
