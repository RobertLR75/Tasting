using FluentMigrator;

namespace Tasting.Api.Infrastructure.Migrations;

[Migration(202608031801)]
public sealed class CreateArrangementTables : Migration
{
    public override void Up()
    {
        Create.Table("arrangements")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("status").AsString(20).NotNullable()
            .WithColumn("row_version").AsInt64().NotNullable().WithDefaultValue(0)
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("updated_at_utc").AsDateTimeOffset().Nullable();

        Create.Index("ix_arrangements_status")
            .OnTable("arrangements")
            .OnColumn("status").Ascending();

        Create.Table("arrangement_participants")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("arrangement_id").AsGuid().NotNullable()
                .ForeignKey("fk_arrangement_participants_arrangement", "arrangements", "id")
            .WithColumn("user_id").AsGuid().NotNullable()
            .WithColumn("first_name_snapshot").AsString(200).NotNullable()
            .WithColumn("last_name_snapshot").AsString(200).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable();

        Create.Index("uix_arrangement_participants_arrangement_user")
            .OnTable("arrangement_participants")
            .OnColumn("arrangement_id").Ascending()
            .OnColumn("user_id").Ascending()
            .WithOptions().Unique();

        Create.Table("arrangement_beers")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("arrangement_id").AsGuid().NotNullable()
                .ForeignKey("fk_arrangement_beers_arrangement", "arrangements", "id")
            .WithColumn("beer_id").AsGuid().NotNullable()
            .WithColumn("name_snapshot").AsString(200).NotNullable()
            .WithColumn("brewery_name_snapshot").AsString(200).NotNullable()
            .WithColumn("beer_style_snapshot").AsString(200).NotNullable()
            .WithColumn("beer_type_snapshot").AsString(200).NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable();

        Create.Index("uix_arrangement_beers_arrangement_beer")
            .OnTable("arrangement_beers")
            .OnColumn("arrangement_id").Ascending()
            .OnColumn("beer_id").Ascending()
            .WithOptions().Unique();
    }

    public override void Down()
    {
        Delete.Table("arrangement_beers");
        Delete.Table("arrangement_participants");
        Delete.Table("arrangements");
    }
}
